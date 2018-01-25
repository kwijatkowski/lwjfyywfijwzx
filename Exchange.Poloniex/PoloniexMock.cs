using Exchange.MarketUtils;
using Exchange.MarketUtils.Mock;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Poloniex
{
    public class PoloniexMock : Poloniex
    {
        public PoloniexMock(string baseAddress, string feesJson, decimal accountMonthlyVolume) : base(baseAddress, feesJson, accountMonthlyVolume)
        {
        }

        public override async Task<Ticker> GetTicker(string currency1, string currency2)
        {
            bool inverted = false;
            Tuple<string, string> orderedPair = MakeValidPair(currency1, currency2, out inverted);
            string pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);
            var candleAsString = GetHistoricalDataMock.GetHistoricalData(pair, 1);
            var candles = JsonConvert.DeserializeObject<List<Candle>>(candleAsString);

            Ticker toReturn;

            if (candles.FirstOrDefault() != null)
            {
                toReturn = new Ticker()
                {
                    ask = candles.FirstOrDefault().Close,
                    bid = candles.FirstOrDefault().Close,
                    last = candles.FirstOrDefault().Close,
                    min = candles.FirstOrDefault().Close,
                    max = candles.FirstOrDefault().Close
                };
            }
            else
            {
                toReturn = new Ticker()
                {
                    bid = int.MinValue
                };
            }

            return toReturn;
        }

        public override async Task<Tuple<Tuple<string, string>, string>> GetHistoricalData(Tuple<string, string> cryptoPair, DateTime start, DateTime end, int periodSeconds)
        {
            long startUnix = UnixTimestamp.ToUnixTimestamp(start);
            long endUnix = end == DateTime.MaxValue ? 9999999999 : UnixTimestamp.ToUnixTimestamp(end);

            bool inverted = false;
            var orderedPair = MakeValidPair(cryptoPair.Item1, cryptoPair.Item2, out inverted);
            string pair;

            if (inverted)
                pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item2, orderedPair.Item1);
            else
                pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);

            var historicalData = GetHistoricalDataMock.GetHistoricalData(pair, (int)(endUnix - startUnix) / (periodSeconds > 0 ? periodSeconds : 1));

            //SerializeUtility.SerializeObject(historicalData, $"{pair}.xml");

            return new Tuple<Tuple<string, string>, string>(cryptoPair, historicalData);
        }
    }
}
