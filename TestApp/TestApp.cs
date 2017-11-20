﻿using Exchange.BitBay;
using Exchange.Kraken;
using Exchange.Poloniex;
using Exchange.MarketUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Strategy.Arbitrage;

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

                List<Tuple<string, string>> tradingPairs = new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>(Currencies.USD,Currencies.Ethereum),
                    new Tuple<string, string>(Currencies.USD,Currencies.Bitcoin),
                    new Tuple<string, string>(Currencies.USD,Currencies.BitcoinCash)
                };

                string startCurrency = Currencies.USD;
                string transferCurrency = Currencies.Ethereum;

                var bbConfig = new ExchangeConfig();
                bbConfig.Load(bitbayConfigPath);

                var krakenConfig = new ExchangeConfig();
                krakenConfig.Load(krakenConfigPath);

                var bitbay = new BitBay(bbConfig);
                var kraken = new Kraken(krakenConfig);


                List<IExchange> exchanges = new List<IExchange>() {
                    bitbay,kraken
                    //new Poloniex("https://poloniex.com/public"),
            };

                ArbitrageStrategy strategy = new ArbitrageStrategy(10);

                foreach (var pair in tradingPairs)
                {
                    Profit profit = await strategy.CalculateProfitForPairAndExchange(pair.Item1, pair.Item2, bitbay, kraken);
                    Console.WriteLine($"{bitbay.GetName()} -> {kraken.GetName()} pair {pair.Item1} {pair.Item2} profit: {profit.absoluteValue} {profit.currency} or {profit.percent}");
                }
                

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

            //var sortedByPrifit = CalculatePriceDifference(tickers);

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