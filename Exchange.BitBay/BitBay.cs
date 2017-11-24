using Exchange.MarketUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exchange.BitBay
{
    public class BitBay : IExchange
    {
        private readonly string _publicApiURL;
        private readonly string _privateApiURL;
        private readonly string _feesJson;
        private decimal _accountMonthlyVolume;

        private PublicApiConnector _publicApiConnector;

        public BitBay(ExchangeConfig config, decimal accountMonthlyVolume, string feesJson)
        {
            _publicApiURL = config.publicApiAddress;
            _privateApiURL = config.privateApiAddress;
            _feesJson = feesJson;
            _accountMonthlyVolume = accountMonthlyVolume;

            _publicApiConnector = new PublicApiConnector(_publicApiURL);
        }

        public string GetName()
        {
            return "BitBay";
        }

        public async Task<Ticker> GetTicker(string currency1, string currency2)
        {
            bool inverted = false;
            var orderedPair = MakeValidPair(currency1, currency2, out inverted);

            if (orderedPair == null)
                return null;

            string pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);

            var bitbayTicker = await _publicApiConnector.GetTicker(pair);

            CheckResponseAndThrow(bitbayTicker, $" market: {currency1} {currency2}");

            BitBayTicker ticker = JsonConvert.DeserializeObject<BitBayTicker>(bitbayTicker);

            Ticker t = new Ticker()
            {
                min = ticker.min,
                max = ticker.max,
                ask = ticker.ask,
                bid = ticker.bid,
                last = ticker.last
            };

            if (inverted)
                return t.Invert(t);
            else
                return t;
        }

        private void CheckResponseAndThrow(string response, string addData = "")
        {
            JObject jo = JObject.Parse(response);            

            if (jo["code"] != null)
                throw new Exception($"Bitbay api error code: {jo["code"].ToString()} message: {jo["message"].ToString()} {addData}");
        }

        public async Task<OrderBook> GetOrderbook(string c1, string c2, int? countLimit = null)
        {
            bool inverted = false;
            var orderedPair = MakeValidPair(c1, c2, out inverted);

            if (orderedPair == null)
                return null;

            string pair = CurrenciesNamesMap.MapNamesToPair(orderedPair.Item1, orderedPair.Item2);

            var resultJson = await _publicApiConnector.GetOrderbook(pair);

            CheckResponseAndThrow(resultJson, $" market: {c1} {c2}");

            OrderBook ob = bitBayOrderbookToOrderbook(c1, c2, resultJson);

            if (inverted)
                return ob.Invert(ob);
            else
                return ob;
        }

        private List<string> GetTradablePairs()
        {
            return _publicApiConnector.GetTradablePairs().GetAwaiter().GetResult();
            //throw new NotImplementedException();
        }

        public decimal CalculateTransacionFee(string startCurrency, string targetCurrency)
        {
            string pair = CurrenciesNamesMap.MapNamesToPair(startCurrency, targetCurrency);

            OperationCostCalculator calc = new OperationCostCalculator(_feesJson, _accountMonthlyVolume); //no api, load from hdd
            return calc.CalculateTransactionFee(pair, OperationTypes.OPERATION_TYPE.taker); //todo: select transaction fee based on something
        }

        public decimal CalculateTransferFee(string currency, decimal volume = 0)
        {
            string pair = CurrenciesNamesMap.MapNameToSymbol(currency);

            OperationCostCalculator calc = new OperationCostCalculator(_feesJson, _accountMonthlyVolume); //no api, load from hdd
            return calc.CalculateTransferCost(pair, OperationTypes.TRANSFER_DIR.outgoing); //todo: select transaction fee based on something
        }

        public Tuple<string,string> MakeValidPair(string currency1, string currency2, out bool inverted)
        {
            string symbol1 = CurrenciesNamesMap.MapNameToSymbol(currency1);
            string symbol2 = CurrenciesNamesMap.MapNameToSymbol(currency2);

            List<string> tradablePairs = GetTradablePairs();
            if (tradablePairs.Any(i => i == (symbol1 + symbol2)))
            {
                inverted = false;
                return new Tuple<string, string>(currency1, currency2);
            }
            else if (tradablePairs.Any(i => i == (symbol2 + symbol1)))
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

        private OrderBook bitBayOrderbookToOrderbook(string c1, string c2, string orderbookJson)
        {
            var bitBayOrderBook = JsonConvert.DeserializeObject<BitBayOrderBook>(orderbookJson);

            var orderBook = new OrderBook(c1, c2);

            for (int i = 0; i < bitBayOrderBook.bids.GetLength(0); i++)
                orderBook.bids.Add(new Bid() { price = bitBayOrderBook.bids[i, 0], volume = bitBayOrderBook.bids[i, 1] });

            for (int i = 0; i < bitBayOrderBook.asks.GetLength(0); i++)
                orderBook.asks.Add(new Ask() { price = bitBayOrderBook.asks[i, 0], volume = bitBayOrderBook.asks[i, 1] });

            return orderBook;
        }
    }
}