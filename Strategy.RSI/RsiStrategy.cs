using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;
using Exchange.Poloniex;
using log4net;
using Newtonsoft.Json;
using System.Threading;

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

        private int exchangeOperationsCheckDelay = 1000;
        private decimal volumeTreshold;


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

        public async Task Run()
        {
            DateTime end = DateTime.MaxValue;

            if (STATE.LOOKING_FOR_OPPORTUNITY == currentState)
            {
                //implement volume treshold - take it from the poloniex public api connector public async Task<string> Get24hVolume()
                bestPair = await FindLowestRsiPair(end);
                volumeTreshold = await _exchange.GetVolumeThreshold(bestPair.Item1, bestPair.Item2);

                if (bestPair.Item3 <= _buyTreshold)
                {
                    Ticker t = _exchange.GetTicker(bestPair.Item1, bestPair.Item2).GetAwaiter().GetResult();

                    currentState = STATE.BUYING;
                    buyPrice = t.ask;
                    sellPrice = buyPrice * (1 + _targetProfitPercentage);

                    var vThreshold = Math.Round(_currentBalance >= volumeTreshold ? volumeTreshold : _currentBalance, 8);
                    SetBuyOrder(bestPair.Item1, bestPair.Item2, t.ask, vThreshold);
                    _logger.Debug($"{DateTime.Now} Buy order set {bestPair.Item1} {bestPair.Item2} price {t.ask} volume {vThreshold}");

                    while (! await IsBuyOrderFilled(bestPair.Item1, bestPair.Item2))
                    {
                        await Task.Delay(exchangeOperationsCheckDelay);
                    }

                    currentState = STATE.IN_POSITION;
                    _logger.Debug($"{DateTime.Now} Buy order filled {bestPair.Item1} {bestPair.Item2} price {t.ask}");
                }
            }

            else if (currentState == STATE.IN_POSITION)
            {
                //sell
                SetSellOrder(bestPair.Item1, bestPair.Item2, sellPrice);
                _logger.Debug($"{DateTime.Now} Sell order set {bestPair.Item1} {bestPair.Item2} price {sellPrice}");
                currentState = STATE.SELLING;

                while (!await IsSellOrderFilled(bestPair.Item1, bestPair.Item2))
                {
                    await Task.Delay(exchangeOperationsCheckDelay);
                }

                _logger.Debug($"{DateTime.Now} Sell order filled {bestPair.Item1} {bestPair.Item2} price {sellPrice}");
                _logger.Debug($"Sold {bestPair.Item1} {bestPair.Item2} @ {sellPrice}, profit {sellPrice - buyPrice}");

                _currentBalance = _currentBalance * sellPrice/buyPrice; //temporary

                currentState = STATE.LOOKING_FOR_OPPORTUNITY;
            }
        }

        private async void SetBuyOrder(string currency1, string currency2, decimal price, decimal amount)
        {

        }

        //sell max
        private async void SetSellOrder(string currency1, string currency2, decimal price)
        {

        }

        private async Task<decimal> CheckAccountBalance()
        {
            return _currentBalance;
        }

        private async Task<bool> IsSellOrderFilled(string currency1, string currency2)
        {
            Ticker t = _exchange.GetTicker(bestPair.Item1, bestPair.Item2).GetAwaiter().GetResult();


            if (sellPrice <= t.bid)
                return true;
            else
                return false;
        }

        private async Task<bool> IsBuyOrderFilled(string currency1, string currency2)
        {
            return true;
        }

        private async Task<Tuple<string,string,decimal>> FindLowestRsiPair(DateTime end)
        {
            decimal lastLowestRSI = 100;
            Tuple<string, string> bestPair = new Tuple<string, string>("", "");

            DateTime startDate = DateTime.UtcNow - new TimeSpan(0, 0, (_period + 1) * _candlePeriod);
            var tmp = new List<Task<Tuple<Tuple<string, string>, string>>>();

            _currenciesToWorkOn.ForEach(pair => tmp.Add(_exchange.GetHistoricalData(pair, startDate, end, _candlePeriod)));

            var allHistoricalData = await Task.WhenAll(tmp);
           
            foreach (var singleCrypto in allHistoricalData)
            {
                var pair = singleCrypto.Item1;
                var candlesJson = singleCrypto.Item2;

                List<Candle> candles = JsonConvert.DeserializeObject<List<Candle>>(candlesJson);
                List<decimal> prices = candles.Select(p => p.Close).ToList();

                Exchange.MarketUtils.RSI rsiCalc = new Exchange.MarketUtils.RSI();

                decimal rsi = rsiCalc.CalculateRSI(prices, _period);

                if (rsi < lastLowestRSI)
                {
                    lastLowestRSI = rsi;
                    bestPair = pair;
                }
                _logger.Debug($"Pair: {pair.Item1} {pair.Item2} rsi {rsi}");
            }

            if(bestPair != null)
                _logger.Debug($"bestPair: {bestPair.Item1} {bestPair.Item2} rsi {lastLowestRSI}");
            //buy @ current price

            //set sell order @ higher price
            return new Tuple<string, string, decimal>(bestPair.Item1, bestPair.Item2, lastLowestRSI);
        }

    }
}
