using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using Exchange.MarketUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Exchange.Poloniex;

namespace Exchange.Mock
{
    public class PublicApiConnectorMock : IPublicApiConnector
    {
        //documentation
        //https://m.poloniex.com/support/api/

        private readonly string _historicalDataDir;
        private readonly string _24hvolumeFile;
        private Dictionary<Tuple<string, string>, List<Candle>> _historicalDataCollection; //key is currency1 and currency2 value is historical data json
        private ITimeProvider _timeProvider;

        public PublicApiConnectorMock(string historicalDataDir,string volumeFile, ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _historicalDataDir = historicalDataDir;
            _24hvolumeFile = volumeFile;
            _historicalDataCollection = new Dictionary<Tuple<string, string>, List<Candle>>();

            var files = Directory.EnumerateFiles(_historicalDataDir).Where(f =>  Path.GetExtension(f) == ".json");

            foreach (var fileName in files)
            {
                var historicalDataJson = File.ReadAllText(fileName);
                string name = Path.GetFileName(fileName);

                var pairArry = name.Split('_').ToArray(); // file name is in following convention: currency1_currency2_year_candle_period.json                                
                _historicalDataCollection.Add(new Tuple<string, string>(pairArry[0], pairArry[1]), JsonConvert.DeserializeObject<List<Candle>>(historicalDataJson));
            }
        }

        //Appropriate labels for these data are, in order: currencyPair, last, lowestAsk, highestBid, percentChange, baseVolume, quoteVolume, isFrozen, 24hrHigh, 24hrLow
        public async Task<string> GetTicker(string currency1 = null, string currency2 = null)
        {            
            Dictionary<string, Ticker> allCryptoTicer = new Dictionary<string, Ticker>();

            if (currency1 != null && currency2 != null)
            {
                var set = _historicalDataCollection.First(e => e.Key.Item1 == currency1 && e.Key.Item2 == currency2);
                string pair = CurrenciesNamesMap.MapNamesToPair(set.Key.Item1, set.Key.Item2);
                List<Candle> candles = set.Value;

                int timestamp = UnixTimestamp.ToUnixTimestamp(_timeProvider.Now());
                var tickerCandle = candles.Where(c => c.Date >= timestamp).First();

                Ticker t = new Ticker();
                t.ask = tickerCandle.High;
                t.bid = tickerCandle.Low;
                t.currency1 = set.Key.Item1;
                t.currency2 = set.Key.Item2;
                t.last = (tickerCandle.Open + tickerCandle.Close) / 2;
                t.max = tickerCandle.High;
                t.min = tickerCandle.Low;

                allCryptoTicer.Add(pair, t);

            }
            else
            {
                foreach (var set in _historicalDataCollection)
                {
                    string pair = CurrenciesNamesMap.MapNamesToPair(set.Key.Item1, set.Key.Item2);
                    List<Candle> candles = set.Value;

                    int timestamp = UnixTimestamp.ToUnixTimestamp(_timeProvider.Now());
                    var tickerCandle = candles.Where(c => c.Date >= timestamp).First();

                    Ticker t = new Ticker();
                    t.ask = tickerCandle.High;
                    t.bid = tickerCandle.Low;
                    t.currency1 = set.Key.Item1;
                    t.currency2 = set.Key.Item2;
                    t.last = (tickerCandle.Open + tickerCandle.Close) / 2;
                    t.max = tickerCandle.High;
                    t.min = tickerCandle.Low;

                    allCryptoTicer.Add(pair, t);
                }

            }
            string serialized =  JsonConvert.SerializeObject(allCryptoTicer);

            return serialized;
            //string relative = string.Empty;
            //Dictionary<string, string> parameters = new Dictionary<string, string>()
            //{
            //    { "command", "returnTicker"}
            //};
            //
            //return await GetDataFromAddress<string>(BuildRequestUrl(_historicalDataDir, relative, parameters));
        }

        public async Task<string> GetCurrencies()
        {
            throw new NotImplementedException();

            //string relative = string.Empty;
            //Dictionary<string, string> parameters = new Dictionary<string, string>()
            //{
            //    { "command", "returnCurrencies"}
            //};
            //
            //return await GetDataFromAddress<string>(BuildRequestUrl(_historicalDataDir, relative, parameters));
        }

        //https://poloniex.com/public?command=return24hVolume
        public async Task<string> Get24hVolume()
        {
            return File.ReadAllText(_24hvolumeFile);
        }

        public async Task<string> GetOrderBook(string pair, int? count = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair">Must be in c1_c2 format</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public async Task<string> GetChartData(string pair, long start, long end, int period)
        {
            var currencies = pair.Split('_');
            string c1 = CurrenciesNamesMap.SymbolToName(currencies[0]);
            string c2 = CurrenciesNamesMap.SymbolToName(currencies[1]);

            if (!_historicalDataCollection.Any(e => e.Key.Item1 == c1 && e.Key.Item2 == c2))
                throw new Exception("Historical data for pair {c1} {c2} not available");

                List<Candle> candles = _historicalDataCollection.First(e => e.Key.Item1 == c1 && e.Key.Item2 == c2).Value;
            //return pairHistoricalData;

         //   JObject j = JObject.Parse(pairHistoricalData);

            string chartData = JsonConvert.SerializeObject(candles.Where(c => c.Date >= start && c.Date <= end));
            //int count j.Descendants().Count(e => Convert.ToInt32(e["date"]) > 1483239600)
            return chartData;
        }
    }

}
