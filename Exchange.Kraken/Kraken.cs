using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Kraken
{
    public class Kraken : IExchange
    {
        private string _publicApiURL;
        private string _privateApiURL;

        private PublicApiConnector publicApiConnector;
        //private PrivateApiConnector publicApiConnector;

        public Kraken(string publicApiURL, string privateApiURL)
        {
            _publicApiURL = publicApiURL;
            _privateApiURL = privateApiURL;

            publicApiConnector = new PublicApiConnector(_publicApiURL);
        }

        public string GetName()
        {
            return "Kraken";
        }

        public async Task<Dictionary<string, KrakenTicker>> GetKrakenTicker(string currency1, string currency2)
        {
            string pair = string.Concat(CurrenciesNamesMap.MapName(currency1), CurrenciesNamesMap.MapName(currency2));
            string tickerJson = await publicApiConnector.GetTicker(pair);
            JObject j = JObject.Parse(tickerJson);
            return JsonConvert.DeserializeObject<Dictionary<string, KrakenTicker>>(j.SelectToken("result").ToString());
        }

        public async Task<Ticker> GetTicker(string currency1, string currency2)
        {
            KrakenTicker ct = (await GetKrakenTicker(currency1, currency2)).First().Value;

            Ticker t = new Ticker()
            {
                min = ct.l[1],
                max = ct.h[1],
                ask = ct.a[0],
                bid = ct.b[0],
                last = ct.c[0]
            };
            return t;
        }

        public void GetOrderbook()
        {
            throw new NotImplementedException();
        }
    }
}
