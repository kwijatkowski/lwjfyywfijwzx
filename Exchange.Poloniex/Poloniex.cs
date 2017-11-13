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

        public Poloniex(string baseAddress)
        {
            _publicApiURL = baseAddress;
            _publicApiConnector = new PublicApiConnector(_publicApiURL);
        }
        public string GetName()
        {
            return "Poloniex";
        }

        public async Task<Ticker> GetTicker(string currency1, string currency2)
        {
            string mapped1 = CurrenciesNamesMap.MapName(currency1);
            string mapped2 = CurrenciesNamesMap.MapName(currency2);

            string response = await _publicApiConnector.GetTicker();

            var allCurrenciesTicker = await _publicApiConnector.GetTicker();
            //JObject jo = JObject.Parse(allCurrenciesJson);
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, PoloniexTicker>>(allCurrenciesTicker);

            PoloniexTicker ticker = null;

            if (deserialized.TryGetValue(string.Concat(mapped1, "_", mapped2), out ticker))
            {

                return new Ticker()
                {
                    ask = ticker.lowestAsk,
                    bid = ticker.highestBid,
                    last = ticker.last,
                    min = ticker.low24hr,
                    max = ticker.high24hr
                };
            }
            else
            {
                throw new Exception($"Unable to get ticker for {currency1} and {currency2} at {GetName()}");
            }
        }

        public async Task<OrderBook> GetOrderbook(string currency1, string currency2, decimal bidLimit, decimal askLimit, int? countLimit = null)
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string,string>> GetCurrenciesMap()
        {
            var allCurrenciesInfo = await _publicApiConnector.GetCurrencies();

            Dictionary<string, PoloniexCurrenciesInfo> info = JsonConvert.DeserializeObject<Dictionary<string,PoloniexCurrenciesInfo>>(allCurrenciesInfo);

            var map = new Dictionary<string, string>();

            foreach (var item in info)
                map.Add(item.Value.name, item.Key);

            var ser = JsonConvert.SerializeObject(map);
            return null;
        }

        public List<Tuple<string, string>> GetTradablePairs()
        {
            throw new NotImplementedException();
        }
    }
}
