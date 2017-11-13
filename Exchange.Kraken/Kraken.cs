using Exchange.MarketUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<OrderBook> GetOrderbook(string currency1, string currency2, decimal lowLimit, decimal topLimit, int? limit = null)
        {
            string pair = string.Concat(CurrenciesNamesMap.MapName(currency1), CurrenciesNamesMap.MapName(currency2));

            var parameters = new Dictionary<string, string>();
            parameters.Add("pair", pair);
            if (limit != null)
                parameters.Add("count", limit.ToString());

            var orderbookJson = await _publicApiConnector.GetOrderBook(parameters);

            JObject j = JObject.Parse(orderbookJson);
            CheckErrorsAndThrow(j);

            var resultJson = j.SelectToken("result").ToString();

            return KrakenOrderBookToOrderBook(resultJson, lowLimit, topLimit);
        }

        private OrderBook KrakenOrderBookToOrderBook(string json, decimal bidLimit, decimal askLimit)
        {
            var krakenOrderBook = JsonConvert.DeserializeObject<Dictionary<string, KrakenOrderBook>>(json).Values.First();
            var orderBook = new OrderBook();

            for (int i = 0; i < krakenOrderBook.asks.GetLength(0); i++)
                orderBook.asks.Add(new Ask() { price = krakenOrderBook.asks[i, 0], volume = krakenOrderBook.asks[i, 0] });

            for (int i = 0; i < krakenOrderBook.bids.GetLength(0); i++)
                orderBook.bids.Add(new Bid() { price = krakenOrderBook.bids[i, 0], volume = krakenOrderBook.bids[i, 0] });

            //filter outliers
            orderBook.bids = orderBook.bids.Where(b => b.price > bidLimit).ToList();
            orderBook.asks = orderBook.asks.Where(a => a.price < askLimit).ToList();

            return orderBook;
        }

        private void CheckErrorsAndThrow(JObject response)
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

        public List<Tuple<string, string>> GetTradablePairs()
        {
            return new List<Tuple<string, string>>()
            {
                //Tradeable against ETH
                new Tuple<string,string>("XBT","ETH"),
                new Tuple<string,string>("CAD","ETH"),
                new Tuple<string,string>("EUR","ETH"),
                new Tuple<string,string>("GBP","ETH"),
                new Tuple<string,string>("JPY","ETH"),
                new Tuple<string,string>("USD","ETH"),

                //Tradeable against LTC
                new Tuple<string,string>("XBT","LTC"),
                new Tuple<string,string>("EUR","LTC"),
                new Tuple<string,string>("USD","LTC"),

                //Tradeable against BCH
                new Tuple<string,string>("USD","BCH"),
                new Tuple<string,string>("EUR","BCH"),
                new Tuple<string,string>("XBT","BCH"),

                //Tradeable against DASH
                new Tuple<string,string>("USD","DASH"),
                new Tuple<string,string>("EUR","DASH"),
                new Tuple<string,string>("XBT","DASH"),

                //Tradeable against EOS
                new Tuple<string,string>("USD","EOS"),
                new Tuple<string,string>("EUR","EOS"),
                new Tuple<string,string>("XBT","EOS"),
                new Tuple<string,string>("ETH","EOS"),

                //Tradeable against ETC
                new Tuple<string,string>("USD","ETC"),
                new Tuple<string,string>("EUR","ETC"),
                new Tuple<string,string>("XBT","ETC"),
                new Tuple<string,string>("ETH","ETC"),

                //Tradeable against GNO
                new Tuple<string,string>("USD","GNO"),
                new Tuple<string,string>("EUR","GNO"),
                new Tuple<string,string>("XBT","GNO"),
                new Tuple<string,string>("ETH","GNO"),

                //Tradeable against ICN
                new Tuple<string,string>("XBT","ICN"),
                new Tuple<string,string>("ETH","ICN"),

                //Tradeable against MLN
                new Tuple<string,string>("XBT","MLN"),
                new Tuple<string,string>("ETH","MLN"),

                //Tradeable against REP
                new Tuple<string,string>("USD","REP"),
                new Tuple<string,string>("EUR","REP"),
                new Tuple<string,string>("XBT","REP"),
                new Tuple<string,string>("ETH","REP"),

                //Tradeable against XDG
                new Tuple<string,string>("XBT","XDG"),

                //Tradeable against XLM
                new Tuple<string,string>("USD","XLM"),
                new Tuple<string,string>("EUR","XLM"),
                new Tuple<string,string>("XBT","XLM"),

                //Tradeable against XMR
                new Tuple<string,string>("USD","XMR"),
                new Tuple<string,string>("EUR","XMR"),
                new Tuple<string,string>("XBT","XMR"),

                //Tradeable against XRP
                new Tuple<string,string>("USD","XRP"),
                new Tuple<string,string>("EUR","XRP"),
                new Tuple<string,string>("XBT","XRP"),
                new Tuple<string,string>("CAD","XRP"),
                new Tuple<string,string>("JPY","XRP"),

                //Tradeable against ZEC
                new Tuple<string,string>("USD","ZEC"),
                new Tuple<string,string>("EUR","ZEC"),
                new Tuple<string,string>("XBT","ZEC"),

                //Tradeable against XBT
                new Tuple<string,string>("BCH","XBT"),
                new Tuple<string,string>("CAD","XBT"),
                new Tuple<string,string>("EUR","XBT"),
                new Tuple<string,string>("GBP","XBT"),
                new Tuple<string,string>("JPY","XBT"),
                new Tuple<string,string>("USD","XBT")
        };
    }
}
}