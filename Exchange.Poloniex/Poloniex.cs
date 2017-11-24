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
            Tuple<string,string> orderedPair = MakeValidPair(currency1, currency2, out inverted);

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
            throw new NotImplementedException();
        }

        public decimal CalculateTransacionFee(string currency1, string currency2)
        {
            bool inverted = false;
            var orderedPair = MakeValidPair(currency1, currency2, out inverted);
            string pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);

            return decimal.MaxValue;

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
    }
}
