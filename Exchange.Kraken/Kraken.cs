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
        private decimal _accountMonthlyVolume;

        private PublicApiConnector _publicApiConnector;
        //private PrivateApiConnector publicApiConnector;

        public Kraken(ExchangeConfig config, decimal accountMonthlyVolume)
        {
            _publicApiURL = config.publicApiAddress;
            _privateApiURL = config.privateApiAddress;
            _accountMonthlyVolume = accountMonthlyVolume;

            _publicApiConnector = new PublicApiConnector(_publicApiURL);
        }

        public string GetName()
        {
            return "Kraken";
        }

        private async Task<Dictionary<string, KrakenTicker>> GetKrakenTicker(string currency1, string currency2)
        {
            string pair = CurrenciesNamesMap.MapNamesToPair(currency1, currency2);
            string tickerJson = await _publicApiConnector.GetTicker(pair);
            JObject j = JObject.Parse(tickerJson);
            CheckResponseAndThrow(j, $" market: {currency1} {currency2}");
            return JsonConvert.DeserializeObject<Dictionary<string, KrakenTicker>>(j.SelectToken("result").ToString());
        }

        public async Task<Ticker> GetTicker(string currency1, string currency2)
        {
            var orderedPair = MakeValidPair(currency1, currency2);

            if (orderedPair == null)
                return null;

            KrakenTicker ct = (await GetKrakenTicker(orderedPair.Item1, orderedPair.Item2)).First().Value;

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
            var orderedPair = MakeValidPair(currency1, currency2);

            if (orderedPair == null)
                return null;

            string pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);

            var parameters = new Dictionary<string, string>();
            parameters.Add("pair", pair);
            if (limit != null)
                parameters.Add("count", limit.ToString());

            var orderbookJson = await _publicApiConnector.GetOrderBook(parameters);


            JObject j = JObject.Parse(orderbookJson);
            CheckResponseAndThrow(j, $" market: {currency1} {currency2}");

            var resultJson = j.SelectToken("result").ToString();

            return KrakenOrderBookToOrderBook(orderedPair.Item1, orderedPair.Item2, resultJson, lowLimit, topLimit);
        }

        private OrderBook KrakenOrderBookToOrderBook(string c1, string c2, string json, decimal bidLimit, decimal askLimit)
        {
            var krakenOrderBook = JsonConvert.DeserializeObject<Dictionary<string, KrakenOrderBook>>(json).Values.First();
            var orderBook = new OrderBook(c1, c2);

            for (int i = 0; i < krakenOrderBook.asks.GetLength(0); i++)
                orderBook.asks.Add(new Ask() { price = krakenOrderBook.asks[i, 0], volume = krakenOrderBook.asks[i, 0] });

            for (int i = 0; i < krakenOrderBook.bids.GetLength(0); i++)
                orderBook.bids.Add(new Bid() { price = krakenOrderBook.bids[i, 0], volume = krakenOrderBook.bids[i, 0] });

            //filter outliers
            orderBook.bids = orderBook.bids.Where(b => b.price > bidLimit).ToList();
            orderBook.asks = orderBook.asks.Where(a => a.price < askLimit).ToList();

            return orderBook;
        }

        public decimal CalculateTransacionFee(string currency1, string currency2)
        {
            string pair = CurrenciesNamesMap.MapNamesToPair(currency1, currency2);

            OperationCostCalculator calc = new OperationCostCalculator(_publicApiURL, _accountMonthlyVolume);
            //todo: implement logic to select fee type
            return calc.CalculateTransactionFee(pair, OperationTypes.OPERATION_TYPE.taker);
        }

        public decimal CalculateTransferFee(string transferCurrency, decimal volume = 0)
        {
            OperationCostCalculator calc = new OperationCostCalculator(_publicApiURL, _accountMonthlyVolume);
            return calc.CalculateTransferCost(transferCurrency, OperationTypes.TRANSFER_DIR.outgoing);
        }

        public List<string> GetTradablePairs()
        {
            List<Tuple<string, string>> tradablePairs = new List<Tuple<string, string>>()
            {
                //Tradeable against ETH
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.CAD),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.GBP),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.JPY),
                new Tuple<string,string>(KrakenCurrencies.ETH,KrakenCurrencies.USD),

                //Tradeable against LTC
                new Tuple<string,string>(KrakenCurrencies.LTC,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.LTC,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.LTC,KrakenCurrencies.USD),


                //Tradeable against BCH
                new Tuple<string,string>(KrakenCurrencies.BCH,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.BCH,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.BCH,KrakenCurrencies.XBT),

                //Tradeable against DASH
                new Tuple<string,string>(KrakenCurrencies.DASH,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.DASH,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.DASH,KrakenCurrencies.XBT),

                //Tradeable against EOS
                new Tuple<string,string>(KrakenCurrencies.EOS,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.EOS,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.EOS,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.EOS,KrakenCurrencies.ETH),

                //Tradeable against ETC
                new Tuple<string,string>(KrakenCurrencies.ETC,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.ETC,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.ETC,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.ETC,KrakenCurrencies.ETH),

                //Tradeable against GNO
                new Tuple<string,string>(KrakenCurrencies.GNO,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.GNO,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.GNO,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.GNO,KrakenCurrencies.ETH),

                //Tradeable against ICN
                new Tuple<string,string>(KrakenCurrencies.ICN,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.ICN,KrakenCurrencies.ETH),

                //Tradeable against MLN
                new Tuple<string,string>(KrakenCurrencies.MLN,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.MLN,KrakenCurrencies.ETH),

                //Tradeable against REP
                new Tuple<string,string>(KrakenCurrencies.REP,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.REP,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.REP,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.REP,KrakenCurrencies.ETH),

                //Tradeable against XDG
                new Tuple<string,string>(KrakenCurrencies.XDG,KrakenCurrencies.XBT),

                //Tradeable against XLM
                new Tuple<string,string>(KrakenCurrencies.XLM,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.XLM,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.XLM,KrakenCurrencies.XBT),

                //Tradeable against XMR
                new Tuple<string,string>(KrakenCurrencies.XMR,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.XMR,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.XMR,KrakenCurrencies.XBT),

                //Tradeable against XRP
                new Tuple<string,string>(KrakenCurrencies.XRP,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.XRP,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.XRP,KrakenCurrencies.XBT),
                new Tuple<string,string>(KrakenCurrencies.XRP,KrakenCurrencies.CAD),
                new Tuple<string,string>(KrakenCurrencies.XRP,KrakenCurrencies.JPY),

                //Tradeable against ZEC
                new Tuple<string,string>(KrakenCurrencies.ZEC,KrakenCurrencies.USD),
                new Tuple<string,string>(KrakenCurrencies.ZEC,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.ZEC,KrakenCurrencies.XBT),

                //Tradeable against XBT
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.BCH),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.CAD),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.EUR),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.GBP),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.JPY),
                new Tuple<string,string>(KrakenCurrencies.XBT,KrakenCurrencies.USD)
        };

            var newList = new List<string>(tradablePairs.Count);

            foreach (var pair in tradablePairs)
            {
                newList.Add(
                    CurrenciesNamesMap.MapSymbolsToPair(pair.Item1, pair.Item2)
                        );
            }

            return newList;
        }

        public Tuple<string,string> MakeValidPair(string currency1, string currency2)
        {
            string pair1 = CurrenciesNamesMap.MapNamesToPair(currency1, currency2);
            string pair2 = CurrenciesNamesMap.MapNamesToPair(currency2, currency1);

            List<string> tradablePairs = GetTradablePairs();
            if (tradablePairs.Any(i => i == pair1))
                return new Tuple<string, string>(currency1, currency2);
            else if (tradablePairs.Any(i => i == pair2))
                return new Tuple<string, string>(currency2, currency1);
            else
                return null;
        }

        private void CheckResponseAndThrow(JObject response, string addData = "")
        {
            JArray errors = (JArray)response["error"];
            if (errors.Count > 0)
            {
                string errMsg = string.Empty;

                foreach (var error in errors)
                    errMsg += error + Environment.NewLine;

                throw new System.Exception(errMsg + addData);
            }
        }
    }
}