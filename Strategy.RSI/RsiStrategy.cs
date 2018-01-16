using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;
using Exchange.Poloniex;
using log4net;
using Newtonsoft.Json;

namespace Strategy.RSI
{
    public class RsiStrategy
    {
        private enum STATE { LOOKING_FOR_OPPORTUNITY, BUYING, IN_POSITION, SELLING }
        private Queue<string> _alreadyTraded;
        private Poloniex _exchange;
        private List<Tuple<string, string>> _currenciesToWorkOn;
        private decimal _buyTreshold;
        private int _period;
        private int _candlePeriod;
        private ILog _logger;
        private STATE currentState;

        private decimal _currentBalance;


        private decimal buyPrice;
        private decimal sellPrice;
        private decimal _targetProfitPercentage;
        private Tuple<string, string, decimal> bestPair;

        public RsiStrategy(Poloniex exchange, List<Tuple<string,string>> currenciesToWorkOn, decimal rsiBuyTreshold, int period, int candlePeriod, decimal targetProfitPercentage, decimal startBalance, ILog logger)
        {
            _exchange = exchange;
            _currenciesToWorkOn = currenciesToWorkOn;
            _buyTreshold = rsiBuyTreshold;
            _period = period;
            _candlePeriod = candlePeriod;
            _logger = logger;
            _targetProfitPercentage = targetProfitPercentage;

            _currentBalance = startBalance;

            currentState = STATE.LOOKING_FOR_OPPORTUNITY;
        }

        public void Run()
        {            

            DateTime end = DateTime.MaxValue;
            // = new Tuple<string, string, decimal>("","", 100);

            if (STATE.LOOKING_FOR_OPPORTUNITY == currentState)
            {
                bestPair = FindLowestRsiPair(end);

                if (bestPair.Item3 <= _buyTreshold)
                {
                    currentState = STATE.BUYING;
                    //buy here
                }
            }
            else if (currentState == STATE.BUYING)
            {
                //check ticker
                Ticker t = _exchange.GetTicker(bestPair.Item1, bestPair.Item2).GetAwaiter().GetResult();
                buyPrice = t.ask;
                sellPrice = buyPrice * (1 + _targetProfitPercentage);
                //check if we bought
                //check if buy order is set
                _logger.Debug($"Bought {bestPair.Item1} {bestPair.Item2} @ {buyPrice}");

                currentState = STATE.IN_POSITION;
            }
            else if (currentState == STATE.IN_POSITION)
            {
                //sell 
                Ticker t = _exchange.GetTicker(bestPair.Item1, bestPair.Item2).GetAwaiter().GetResult();

                if (sellPrice <= t.bid)
                {
                    currentState = STATE.SELLING;
                    decimal profit = (sellPrice / buyPrice);
                    _logger.Debug($"Sold {bestPair.Item1} {bestPair.Item2} @ {sellPrice}, profit {sellPrice - buyPrice}");
                    _currentBalance = _currentBalance * profit;
                }
            }        
            else if (currentState == STATE.SELLING)
            {
                //sell
                currentState = STATE.IN_POSITION;
            }
        }

        private Tuple<string,string,decimal> FindLowestRsiPair(DateTime end)
        {
            decimal lastLowestRSI = 100;
            Tuple<string, string> bestPair = new Tuple<string, string>("", "");

            //find currency with lowest RSI and below treshold
            foreach (var pair in _currenciesToWorkOn)
            {
                DateTime startDate = DateTime.UtcNow - new TimeSpan(0, 0, (_period + 1) * _candlePeriod);
                string candlesJson = _exchange.GetHistoricalData(pair.Item1, pair.Item2, startDate, end, _candlePeriod).GetAwaiter().GetResult();

                List<Candle> candles = JsonConvert.DeserializeObject<List<Candle>>(candlesJson);
                List<decimal> prices = candles.Select(p => p.Close).ToList();

                //foreach(var candle in candles.OrderBy(c => c.))

                Exchange.MarketUtils.RSI rsiCalc = new Exchange.MarketUtils.RSI();

                decimal rsi = rsiCalc.CalculateRSI(prices, _period);

                if (rsi <= _buyTreshold && rsi < lastLowestRSI)
                {
                    lastLowestRSI = rsi;
                    bestPair = pair;
                }
                _logger.Debug($"Pair: {pair.Item1} {pair.Item2} rsi {rsi}");
            }

            return new Tuple<string, string, decimal>(bestPair.Item1, bestPair.Item2, lastLowestRSI);
        }

    }
}
