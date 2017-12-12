﻿using Exchange.BitBay;
using Exchange.Kraken;
using Exchange.Poloniex;
using Exchange.MarketUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Strategy.Arbitrage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using log4net;
using log4net.Core;
using Strategy.SCTP_BREAKOUT;

namespace startup
{
    internal class TestApp
    {
        //public static string privateApiAddress  { get { return "https://bitbay.net/API/Trading/tradingApi.php"; } }
        //public static string publicApiAddress { get { return "https://bitbay.net/API/Public/"; } }
        //public static string publicApiAddress { get { return "https://api.kraken.com/0/public/"; } }
        //public static string privateApiAddress = string.Empty;//{ get { return "https://api.kraken.com/0/public/"; } }

        //public static string configDirPath = @"F:\BestTraderInTheWorld\";
        public static string configDirPath = @"C:\Projects\Priv\Bot\";

        //public static string feesJsonPath = @"F:\BestTraderInTheWorld\Exchange.BitBay\fees.json";
        public static string feesJsonPath = @"C:\Projects\Priv\Bot\Exchange.BitBay\fees.json";
        public static string feesJsonPoloniex = @"C:\Projects\Priv\Bot\Exchange.Poloniex\fees.json";

        public static string bitbayConfigPath = configDirPath + "BitBay.exconf";
        public static string krakenConfigPath = configDirPath + "Kraken.exconf";



        private static void Main(string[] args)
        {
            string marketDataDir = @""
            ILog log = new ConsoleLogger();

            string x = string.Empty;

            //var krakenConfig = new ExchangeConfig();
            //krakenConfig.Load(krakenConfigPath);
            //var kraken = new Kraken(krakenConfig, 0);
            var poloniex = new Poloniex("https://poloniex.com/public", File.ReadAllText(feesJsonPoloniex), 0);

            List<Tuple<string, string>> tradingPairs = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>(Currencies.Bitcoin , Currencies.Ethereum),
                new Tuple<string, string>(Currencies.Bitcoin , Currencies.Litecoin),
                new Tuple<string, string>(Currencies.Bitcoin ,Currencies.BitcoinCash)              
            };

            TimeSpan candleTimeSpan = new TimeSpan(0, 0, 30);
            int ticksInCandle = 10;
            TimeSpan tickTimeSpan = new TimeSpan(0,0,candleTimeSpan.Seconds / ticksInCandle);
            int candlesInTimeframe = 10;
            int totalCandles = ticksInCandle * candlesInTimeframe;                               

            SctpBreakout breakoutStrategy = new SctpBreakout(poloniex, tradingPairs, new TimeSpan(0, 0, 30), totalCandles, new decimal(0.05), log);

            while(true)
            {
                Task.Run(async () =>
                {
                   await breakoutStrategy.Run();
                }).GetAwaiter().GetResult();

                Thread.Sleep(tickTimeSpan.Seconds *1000);
                Console.WriteLine(" ------------------------------------------------------------------------------------------- ");
            }

            #region old
            //while (x != "x")
            //{
            //    Task.Run(async () =>
            //    {
            //        List<string> commonCurrencies = CommonCurrencies(new List<Dictionary<string, string>>() { Exchange.Kraken.CurrenciesNamesMap.MapCrypto, Exchange.BitBay.CurrenciesNamesMap.MapCrypto });

            //        List<Tuple<string, string, decimal>> tradingPairs = new List<Tuple<string, string, decimal>>()
            //        {
            //        new Tuple<string, string,decimal>(Currencies.Bitcoin , Currencies.Ethereum, new decimal(0.5)  )
            //        //new Tuple<string, string>(Currencies.Bitcoin , Currencies.Litecoin   ),
            //        //new Tuple<string, string>(Currencies.Bitcoin ,Currencies.BitcoinCash ),

            //        //new Tuple<string, string, decimal>(Currencies.USD , Currencies.Ethereum, new decimal(1000) ),
            //            //new Tuple<string, string>(Currencies.PLN , Currencies.Bitcoin   )
            //        };

            //        decimal startCurrencyVolume = new decimal(0.04);

            //        var bbConfig = new ExchangeConfig();
            //        bbConfig.Load(bitbayConfigPath);

            //        var krakenConfig = new ExchangeConfig();
            //        krakenConfig.Load(krakenConfigPath);


            //        var bitbay = new BitBay(bbConfig, 0, File.ReadAllText(feesJsonPath));
            //        var kraken = new Kraken(krakenConfig, 0);
            //        var poloniex = new Poloniex("https://poloniex.com/public", File.ReadAllText(feesJsonPath), 0);

            //        List<IExchange> exchanges = new List<IExchange>() {
            //        bitbay,kraken
            //            //new Poloniex("https://poloniex.com/public"),
            //        };

            //        ArbitrageStrategy strategy = new ArbitrageStrategy(3, new decimal(0.3));

            //        foreach (var pair in tradingPairs)
            //        {
            //            //Profit profit = await strategy.CalculateSingleTransferProfitForPairAndExchange(pair.Item1, pair.Item2, bitbay, kraken, pair.Item3);
            //            //Console.WriteLine($"{bitbay.GetName()} -> {kraken.GetName()} pair {pair.Item1} {pair.Item2} profit: {Math.Round(profit.absoluteValue,8)} [{profit.currency}] or {profit.percent.ToString("F")}%");

            //            Profit profit = await strategy.CalculateSingleTransferProfitForPairAndExchange(pair.Item1, pair.Item2, bitbay, poloniex, pair.Item3);
            //            Console.WriteLine($"{bitbay.GetName()} -> {poloniex.GetName()} pair {pair.Item1} {pair.Item2} profit: {Math.Round(profit.absoluteValue, 8)} [{profit.currency}] or {profit.percent.ToString("F")}%");
            //        }
            //    }).GetAwaiter().GetResult();

            //    Thread.Sleep(3000);
            //    Console.WriteLine(" ------------------------------------------------------------------------------------------- ");
            //}
            #endregion           

            Console.WriteLine("Finished...");
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


    public class ConsoleLogger : ILog
    {
        public bool IsDebugEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsFatalEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ILogger Logger
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Debug(object message)
        {
            Console.WriteLine((string)message);
        }

        public void Debug(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void Error(object message)
        {
            throw new NotImplementedException();
        }

        public void Error(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void Fatal(object message)
        {
            throw new NotImplementedException();
        }

        public void Fatal(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void Info(object message)
        {
            throw new NotImplementedException();
        }

        public void Info(object message, Exception exception)
        {
            !!!
        }

        public void InfoFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }

        public void Warn(object message)
        {
            throw new NotImplementedException();
        }

        public void Warn(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, object arg0)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            throw new NotImplementedException();
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            throw new NotImplementedException();
        }
    }
}