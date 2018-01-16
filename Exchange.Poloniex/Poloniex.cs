using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Exchange.Poloniex
{
    public class Poloniex : IExchange
    {
        private string _publicApiURL;
        private PublicApiConnector _publicApiConnector;

        private static List<string> _tradablePairs;
        private static string _feesJson;
        private static decimal _monthlyVolume;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseAddress">Public api address</param>
        /// <param name="feesJson">json containing fees schedule</param>
        /// <param name="accountMonthlyVolume">Currency here is BTC</param>
        public Poloniex(string baseAddress, string feesJson, decimal accountMonthlyVolume)
        {
            _publicApiURL = baseAddress;
            _publicApiConnector = new PublicApiConnector(_publicApiURL);
            _feesJson = feesJson;
            _monthlyVolume = accountMonthlyVolume;
        }

        public string GetName()
        {
            return "Poloniex";
        }

        public async Task<Ticker> GetTicker(string currency1, string currency2)
        {
            bool inverted = false;
            Tuple<string, string> orderedPair = MakeValidPair(currency1, currency2, out inverted);

            if (orderedPair == null)
                return null;

            var allCurrenciesTicker = await _publicApiConnector.GetTicker();
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, PoloniexTicker>>(allCurrenciesTicker);

            PoloniexTicker ticker = null;

            string pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);

            if (!deserialized.TryGetValue(pair, out ticker))
                throw new Exception($"Unable to get ticker for {currency1} and {currency2} at {GetName()}");

            Ticker t = new Ticker()
            {
                ask = ticker.lowestAsk,
                bid = ticker.highestBid,
                last = ticker.last,
                min = ticker.low24hr,
                max = ticker.high24hr
            };

            if (inverted)
                return t.Invert(t);
            else
                return t;
        }

        public async Task<OrderBook> GetOrderbook(string currency1, string currency2, int? limit = null)
        {
            bool inverted = false;
            var orderedPair = MakeValidPair(currency1, currency2, out inverted);
            string pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);

            string book = await _publicApiConnector.GetOrderBook(pair, limit);
            PoloniexOrderBook orderBook = JsonConvert.DeserializeObject<PoloniexOrderBook>(book);

            OrderBook bk = PoloniexOrderBookToOrderBook(orderedPair.Item1, orderedPair.Item2, orderBook);

            if (inverted)
                return bk.Invert(bk);
            else
                return bk;
        }

        public decimal CalculateTransacionFee(string currency1, string currency2)
        {
            bool inverted = false;
            var orderedPair = MakeValidPair(currency1, currency2, out inverted);
            string pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);

            //return decimal.MaxValue;

            return 0;
        }

        public decimal CalculateTransferFee(string transferCurrency, decimal volume = 0)
        {
            OperationCostCalculator occ = new OperationCostCalculator(_feesJson, _monthlyVolume);
            return occ.CalculateTransferCost(transferCurrency, OperationTypes.TRANSFER_DIR.outgoing, volume);
        }

        public List<string> GetTradablePairs()
        {
            //create list of pairs from 24 volume data
            if (_tradablePairs == null)
            {
                string response = _publicApiConnector.Get24hVolume().GetAwaiter().GetResult();
                var pairsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                _tradablePairs = pairsDict.Keys.ToList();
            }

            return _tradablePairs;
        }

        public Tuple<string, string> MakeValidPair(string currency1, string currency2, out bool inverted)
        {
            string pair1 = CurrenciesNamesMap.MapNamesToPair(currency1, currency2);
            string pair2 = CurrenciesNamesMap.MapNamesToPair(currency2, currency1);

            List<string> tradablePairs = GetTradablePairs();
            if (tradablePairs.Any(i => i == pair1))
            {
                inverted = false;
                return new Tuple<string, string>(currency1, currency2);
            }
            else if (tradablePairs.Any(i => i == pair2))
            {
                inverted = true;
                return new Tuple<string, string>(currency2, currency1);
            }
            else
            {
                inverted = false;
                return null;
            }
        }

        private OrderBook PoloniexOrderBookToOrderBook(string currency1, string currency2, PoloniexOrderBook poloniexBook)
        {
            OrderBook book = new OrderBook(currency1, currency2);

            foreach (var ask in poloniexBook.asks)
            {
                book.asks.Add(
                    new Ask
                    {
                        price = Convert.ToDecimal(ask[0],CultureInfo.InvariantCulture),
                        volume = Convert.ToDecimal(ask[1], CultureInfo.InvariantCulture),
                        timestamp = new DateTime()
                    }
                );
            }

            foreach (var bid in poloniexBook.bids)
            {
                book.bids.Add(
                    new Bid
                    {
                        price = Convert.ToDecimal(bid[0], CultureInfo.InvariantCulture),
                        volume = Convert.ToDecimal(bid[1], CultureInfo.InvariantCulture),
                        timestamp = new DateTime()
                    }
                );
            }

            return book;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="start"></param>
        /// <param name="end">now is 9999999999 pass DateTime.MaxValue if you want recent data</param>
        /// <param name="periodSeconds"> valid values are 300, 900, 1800, 7200, 14400, and 86400</param>
        /// <returns></returns>
        public async Task<string> GetHistoricalData(string currency1, string currency2, DateTime start, DateTime end, int periodSeconds)
        {
            long startUnix = UnixTimestamp.ToUnixTimestamp(start);
            long endUnix = end == DateTime.MaxValue ? 9999999999 : UnixTimestamp.ToUnixTimestamp(end);
                           

            bool inverted = false;
            var orderedPair = MakeValidPair(currency1, currency2, out inverted);
            string pair;
            
            if (inverted)
                pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item2, orderedPair.Item1);
            else
             pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);

            return await _publicApiConnector.GetChartData(pair, startUnix, endUnix, periodSeconds);
        }
    }
}
