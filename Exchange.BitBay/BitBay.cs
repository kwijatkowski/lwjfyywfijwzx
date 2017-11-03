using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;
using Newtonsoft.Json;

namespace Exchange.BitBay
{
    public class BitBay : IExchange
    {
        private string _publicApiURL;
        private string _privateApiURL;

        
        PublicApiConnector publicApiConnector;

        public BitBay(ExchangeConfig config)
        {
            _publicApiURL = config.publicApiAddress;
            _privateApiURL = config.privateApiAddress;

            publicApiConnector = new PublicApiConnector(_publicApiURL);
        }

        public string GetName()
        {
            return "BitBay";
        }

        public async Task<Ticker> GetTicker(string currency1, string currency2)
        {
            string c1 = CurrenciesNamesMap.MapName(currency1);
            string c2 = CurrenciesNamesMap.MapName(currency2);

            var bitbayTicker = await publicApiConnector.GetTicker(c1, c2);
            BitBayTicker ticker = JsonConvert.DeserializeObject<BitBayTicker>(bitbayTicker);

            Ticker t = new Ticker()
            {
                min = ticker.min,
                max = ticker.max,
                ask = ticker.ask,
                bid = ticker.bid,
                last = ticker.last
            };

            return t;
        }

        public void GetOrderbook()
        {
            throw new NotImplementedException();
        }
    }
}
