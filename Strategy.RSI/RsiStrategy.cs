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
        private enum STATE {  IN_POSITION, LOOKING_FOR_OPPORTUNITY }
        private Queue<string> _alreadyTraded;
        private Poloniex _exchange;
        private List<Tuple<string, string>> _currenciesToWorkOn;
        private decimal _buyTreshold;
        private int _period;
        private int _candlePeriod;
        private ILog _logger;

        public RsiStrategy(Poloniex exchange, List<Tuple<string,string>> currenciesToWorkOn, decimal rsiBuyTreshold, int period, int candlePeriod, ILog logger)
        {
            _exchange = exchange;
            _currenciesToWorkOn = currenciesToWorkOn;
            _buyTreshold = rsiBuyTreshold;
            _period = period;
            _candlePeriod = candlePeriod;
            _logger = logger;
        }

        public async void Run()
        {           
            decimal lastLowestRSI = 100;
            Tuple<string, string> bestPair = null;
            DateTime end = DateTime.MaxValue;

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

                if (rsi <= _buyTreshold && rsi < lastLowestRSI)
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
        }


    }
}
