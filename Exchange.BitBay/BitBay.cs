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


        private PublicApiConnector _publicApiConnector;

        public BitBay(ExchangeConfig config)
        {
            _publicApiURL = config.publicApiAddress;
            _privateApiURL = config.privateApiAddress;

            _publicApiConnector = new PublicApiConnector(_publicApiURL);
        }

        public string GetName()
        {
            return "BitBay";
        }

        public async Task<Ticker> GetTicker(string currency1, string currency2)
        {
            string c1 = CurrenciesNamesMap.MapNameToSymbol(currency1);
            string c2 = CurrenciesNamesMap.MapNameToSymbol(currency2);

            var bitbayTicker = await _publicApiConnector.GetTicker(c1, c2);
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

        public async Task<OrderBook> GetOrderbook(string currency1, string currency2, decimal bidLimit, decimal askLimit, int? countLimit = null)
        {
            var resultJson = await _publicApiConnector.GetOrderbook(
                 CurrenciesNamesMap.MapNameToSymbol(currency1),
                 CurrenciesNamesMap.MapNameToSymbol(currency2)
                 );

            return bitBayOrderbookToOrderbook(resultJson, bidLimit, askLimit, countLimit);
        }

        public List<Tuple<string,string>> GetTradablePairs()
        {
            return _publicApiConnector.GetTradablePairs().GetAwaiter().GetResult();
            //throw new NotImplementedException();
        }
        
        private OrderBook bitBayOrderbookToOrderbook(string orderbookJson, decimal bidLimit, decimal askLimit, int? limit)
        {
           var bitBayOrderBook = JsonConvert.DeserializeObject<BitBayOrderBook>(orderbookJson);
            
            var orderBook = new OrderBook();

            for (int i = 0; i < bitBayOrderBook.bids.GetLength(0) && (limit != null && i < limit) ; i++)
                orderBook.bids.Add(new Bid() { price = bitBayOrderBook.bids[i, 0], volume = bitBayOrderBook.bids[i, 1] });

            for (int i = 0; i < bitBayOrderBook.asks.GetLength(0) && (limit != null && i < limit); i++)
                orderBook.asks.Add(new Ask() { price = bitBayOrderBook.asks[i, 0], volume = bitBayOrderBook.asks[i, 1] });

            //filter outliers
            orderBook.bids = orderBook.bids.Where(b => b.price > bidLimit).ToList();
            orderBook.asks = orderBook.asks.Where(a => a.price < askLimit).ToList();

            return orderBook;
        }

        public bool IsValidPair(string currency1, string currency2)
        {
            string symbol1 = CurrenciesNamesMap.MapNameToSymbol(currency1);
            string symbol2 = CurrenciesNamesMap.MapNameToSymbol(currency2);

            List<Tuple<string,string>> tradablePairs = GetTradablePairs();
            return tradablePairs.Any(pair => (pair.Item1 == symbol1 && pair.Item2 == symbol2) || (pair.Item1 == symbol2 && pair.Item2 == symbol1));
        }
    }
}
