using Exchange.BitBay;
using Exchange.Kraken;
using Exchange.Poloniex;
using Exchange.MarketUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace startup
{
    internal class TestApp
    {
        //public static string privateApiAddress  { get { return "https://bitbay.net/API/Trading/tradingApi.php"; } }
        //public static string publicApiAddress { get { return "https://bitbay.net/API/Public/"; } }
        //public static string publicApiAddress { get { return "https://api.kraken.com/0/public/"; } }
        //public static string privateApiAddress = string.Empty;//{ get { return "https://api.kraken.com/0/public/"; } }

        public static string bitbayConfigPath = @"C:\Projects\Priv\Bot\BitBay.exconf";
        public static string krakenConfigPath = @"C:\Projects\Priv\Bot\Kraken.exconf";

        private static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                List<string> commonCurrencies = CommonCurrencies(new List<Dictionary<string, string>>() { Exchange.Kraken.CurrenciesNamesMap.MapCrypto, Exchange.BitBay.CurrenciesNamesMap.MapCrypto });

                string currency1 = Currencies.Bitcoin;
                string currency2 = Currencies.USD;

                var bbConfig = new ExchangeConfig();
                bbConfig.Load(bitbayConfigPath);

                var krakenConfig = new ExchangeConfig();
                krakenConfig.Load(krakenConfigPath);

                List<IExchange> exchanges = new List<IExchange>() {
                    //new Poloniex("https://poloniex.com/public"),
                    new BitBay(bbConfig),
                    new Kraken(krakenConfig)
            };

                Dictionary<string, decimal> averageFromOrderBooks = new Dictionary<string, decimal>();

                //var bb = new BitBay(bbConfig);
                //var pairs = bb.GetTradablePairs();
                //
                //var ticker = await new Kraken(krakenConfig).GetKrakenTicker(Currencies.Ethereum, Currencies.USD);
                //ticker = await new Kraken(krakenConfig).GetKrakenTicker(Currencies.USD, Currencies.Ethereum);

                foreach (var exchange in exchanges)
                {
                    try
                    {
                        var ticker = await exchange.GetTicker(currency1, currency2);
                        //
                        //var bidLimit = new decimal(0.7) * ticker.last;
                        //var askLimit = new decimal(1.3) * ticker.last;

                        //var orderbook = await exchange.GetOrderbook(currency1, currency2, bidLimit, askLimit, 15);

                        //Console.WriteLine($"{ exchange.GetName()} ask avg: {orderbook.AskWeightAvg.ToString()}");
                        //Console.WriteLine($"{ exchange.GetName()} bid avg: {orderbook.BidWeightAvg.ToString()}");

                            Console.WriteLine(exchange.GetName());
                        var tradable = exchange.GetTradablePairs();

                        foreach(var pair in tradable)
                        {
                            Console.WriteLine($"{pair.Item1} {pair.Item2}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{exchange.GetName()} {ex.Message}");
                        continue;
                    }
                }

                //GetTickersAndCalculatePriceDifferences(exchanges, commonCurrencies, currency2);

                //var poloniex = new Poloniex("https://poloniex.com/public");
                //var result = await poloniex.GetTicker("", currency2);
                //await poloniex.GetCurrenciesMap();

                //string.Format("{0:0.###}",
            }).GetAwaiter().GetResult();

            Console.WriteLine("Finished...");
            Console.ReadLine();
        }

        public static async void GetTickersAndCalculatePriceDifferences(List<IExchange> exchanges, List<string> commonCurrencies, string currency2)
        {
            List<TickerListItem> tickers = new List<TickerListItem>(exchanges.Count * commonCurrencies.Count);

            foreach (string currency in commonCurrencies)
            {
                foreach (var exchange in exchanges)
                {
                    var tickerListItem = new TickerListItem()
                    {
                        Exchange = exchange.GetName(),
                        currency1 = currency,
                        currency2 = currency2,
                        ticker = await exchange.GetTicker(currency, currency2)
                    };

                    tickers.Add(tickerListItem);

                    Console.WriteLine($"{tickerListItem.Exchange} {currency}/{currency2} last {tickerListItem.ticker.last.ToString("F")} min {tickerListItem.ticker.min.ToString("F")} max {tickerListItem.ticker.max.ToString("F")} ask {tickerListItem.ticker.ask.ToString("F")} bid {tickerListItem.ticker.bid.ToString("F")}");
                }
                Console.WriteLine("------------------------------------------------------------------------------------------------------------");
            }

            var sortedByPrifit = CalculatePriceDifference(tickers);

            foreach (var item in sortedByPrifit)
                Console.WriteLine($"{item.Item1}/{item.Item2} {item.Item3} {string.Format("{0:0.###}", item.Item4)}% ==> {item.Item5.ToString("F")}$");
        }

        public static List<Tuple<string, string, string, decimal, decimal>> CalculatePriceDifference(List<TickerListItem> tickerList)
        {
            var percentage = new List<Tuple<string, string, string, decimal, decimal>>();

            var currencies = tickerList.Select(i => i.currency1).Distinct().ToList();

            foreach (string currency in currencies)
            {
                var thisCurrencyTickers = tickerList.Where(i => i.currency1 == currency).ToList();

                for (int i = 0; i < thisCurrencyTickers.Count(); i++)
                    for (int j = i + 1; j < thisCurrencyTickers.Count(); j++)
                    {
                        var ex1Item = thisCurrencyTickers[i];
                        var ex2Item = thisCurrencyTickers[j];

                        decimal l1 = ex1Item.ticker.last;
                        decimal l2 = ex2Item.ticker.last;

                        percentage.Add(new Tuple<string, string, string, decimal, decimal>(
                            ex1Item.Exchange,
                            ex2Item.Exchange,
                            currency,
                            Math.Abs(l1 - l2) / Math.Min(l1,l2) * new decimal(100), 
                            Math.Abs( ex1Item.ticker.last - ex2Item.ticker.last)));
                    }
            }
            return percentage.OrderByDescending(i => i.Item4).ToList();
        }

        public static List<string> CommonCurrencies(List<Dictionary<string, string>> exchangesCurrencies)
        {
            //take exchange with smalles amount of currencies
            Dictionary<string, string> shortestList = exchangesCurrencies.OrderBy(ex => ex.Count).First();

            List<string> commonCurrencies = new List<string>();

            foreach (var currency in shortestList.Keys)
            {
                if (exchangesCurrencies.All(ex => ex.ContainsKey(currency)))
                    commonCurrencies.Add(currency);
            }
            return commonCurrencies;
        }

        public static string FormatJson(string toFormat)
        {
            return JValue.Parse(toFormat).ToString(Formatting.Indented);
        }
    }
}