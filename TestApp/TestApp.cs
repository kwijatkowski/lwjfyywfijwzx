using Exchange.BitBay;
using Exchange.Kraken;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Exchange.MarketUtils;

namespace startup
{
    internal class TestApp
    {
        public static class BitbayAPI
        {
            public static string privateApiAddress  { get { return "https://bitbay.net/API/Trading/tradingApi.php"; } }
            public static string publicApiAddress { get { return "https://bitbay.net/API/Public/"; } }
        }

        public static class KrakenAPI
        {
            public static string publicApiAddress { get { return "https://api.kraken.com/0/public/"; } }
            public static string privateApiAddress = string.Empty;//{ get { return "https://api.kraken.com/0/public/"; } }
        }

        private static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                List<string> commonCurrencies = CommonCurrencies(new List<Dictionary<string, string>>() { Exchange.Kraken.CurrenciesNamesMap.MapCrypto , Exchange.BitBay.CurrenciesNamesMap.MapCrypto });

                //string currency1 = Currencies.Bitcoin;
                string currency2 = Currencies.USD;

                List<IExchange> exchanges = new List<IExchange>() {
                    new BitBay(BitbayAPI.publicApiAddress, BitbayAPI.privateApiAddress),
                    new Kraken(KrakenAPI.publicApiAddress, KrakenAPI.privateApiAddress)
            };

                foreach (string currency in commonCurrencies)
                {
                    foreach (var exchange in exchanges)
                    {
                        string name = exchange.GetName();
                        Ticker ticker = await exchange.GetTicker(currency, currency2);
                        Console.WriteLine($"{name} {currency}/{currency2} last {ticker.last} min {ticker.min} max {ticker.max} ask {ticker.ask} bid {ticker.bid}");
                    }
                    Console.WriteLine("------------------------------------------------------------------------------------------------------------");
                }

            }).GetAwaiter().GetResult();

            Console.ReadLine();
        }

        public static string FormatJson(string toFormat)
        {
            return JValue.Parse(toFormat).ToString(Formatting.Indented);
        }

        public static List<string> CommonCurrencies(List<Dictionary<string,string>> exchangesCurrencies)
        {
            //take exchange with smalles amount of currencies
            Dictionary<string,string> shortestList = exchangesCurrencies.OrderBy(ex => ex.Count).First();

            List<string> commonCurrencies = new List<string>();

            foreach (var currency in shortestList.Keys)
            {
                if (exchangesCurrencies.All(ex => ex.ContainsKey(currency)))
                    commonCurrencies.Add(currency);
            }
            return commonCurrencies;
        }
    }
}