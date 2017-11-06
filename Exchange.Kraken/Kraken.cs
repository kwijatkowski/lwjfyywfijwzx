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

        private PublicApiConnector _publicApiConnector;
        //private PrivateApiConnector publicApiConnector;

        public Kraken(ExchangeConfig config)
        {
            _publicApiURL = config.publicApiAddress;
            _privateApiURL = config.privateApiAddress;

            _publicApiConnector = new PublicApiConnector(_publicApiURL);
        }

        public string GetName()
        {
            return "Kraken";
        }

        public async Task<Dictionary<string, KrakenTicker>> GetKrakenTicker(string currency1, string currency2)
        {
            string pair = string.Concat(CurrenciesNamesMap.MapName(currency1), CurrenciesNamesMap.MapName(currency2));
            string tickerJson = await _publicApiConnector.GetTicker(pair);
            JObject j = JObject.Parse(tickerJson);
            CheckErrorsAndThrow(j);
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

        public void CheckErrorsAndThrow(JObject response)
        {
            JArray errors = (JArray)response["error"];
            if (errors.Count > 0)
            {
                string errMsg = string.Empty;

                foreach (var error in errors)
                    errMsg += error + Environment.NewLine;

                throw new System.Exception(errMsg);
            }
        }
    }
}
