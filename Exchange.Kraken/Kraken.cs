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
            string pair = string.Concat(CurrenciesNamesMap.MapNameToSymbol(currency1), CurrenciesNamesMap.MapNameToSymbol(currency2));
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
            string pair = string.Concat(CurrenciesNamesMap.MapNameToSymbol(currency1), CurrenciesNamesMap.MapNameToSymbol(currency2));

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
            bool addPrefix = true;

            List<Tuple<string,string>> tradablePairs = new List<Tuple<string, string>>()
            {
                //Tradeable against ETH
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.ETH),
                new Tuple<string,string>(KrakenCurrencies.CAD,KrakenCurrencies.ETH),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.ETH),
                new Tuple<string,string>(KrakenCurrencies.GBP,KrakenCurrencies.ETH),
                new Tuple<string,string>(KrakenCurrencies.JPY,KrakenCurrencies.ETH),
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.ETH),

                //Tradeable against LTC
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.LTC),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.LTC),
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.LTC),

                //Tradeable against BCH
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.BCH),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.BCH),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.BCH),

                //Tradeable against DASH
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.DASH),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.DASH),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.DASH),

                //Tradeable against EOS
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.EOS),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.EOS),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.EOS),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.EOS),

                //Tradeable against ETC
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.ETC),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.ETC),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.ETC),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.ETC),

                //Tradeable against GNO
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.GNO),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.GNO),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.GNO),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.GNO),

                //Tradeable against ICN
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.ICN),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.ICN),

                //Tradeable against MLN
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.MLN),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.MLN),

                //Tradeable against REP
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.REP),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.REP),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.REP),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.REP),

                //Tradeable against XDG
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.XDG),

                //Tradeable against XLM
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.XLM),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.XLM),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.XLM),

                //Tradeable against XMR
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.XMR),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.XMR),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.XMR),

                //Tradeable against XRP
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.XRP),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.XRP),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.XRP),
                new Tuple<string,string>(KrakenCurrencies.CAD,KrakenCurrencies.XRP),
                new Tuple<string,string>(KrakenCurrencies.JPY,KrakenCurrencies.XRP),

                //Tradeable against ZEC
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.ZEC),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.ZEC),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.ZEC),

                //Tradeable against XBT
                new Tuple<string,string>(KrakenCurrencies.BCH,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.CAD,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.EUR,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.GBP,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.JPY,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.USD,KrakenCurrencies.XBT)
        };

            if (addPrefix)
            {
                var newList = new List<Tuple<string, string>>(tradablePairs.Count);

                foreach(var pair in tradablePairs)
                {
                    newList.Add(
                        new Tuple<string, string>(
                            CurrenciesNamesMap.GetPrefix(pair.Item1) + pair.Item1,
                            CurrenciesNamesMap.GetPrefix(pair.Item2) + pair.Item2
                            )
                            );
                }

                return newList;
}
            else
            {
                return tradablePairs;
            }
    }

        public bool IsValidPair(string currency1, string currency2)
        {
            string symbol1 = CurrenciesNamesMap.MapNameToSymbol(currency1);
            string symbol2 = CurrenciesNamesMap.MapNameToSymbol(currency2);

            List<Tuple<string, string>> tradablePairs = GetTradablePairs();
            return tradablePairs.Any(pair => (pair.Item1 == symbol1 && pair.Item2 == symbol2) || (pair.Item1 == symbol2 && pair.Item2 == symbol1));
        }
    }
}