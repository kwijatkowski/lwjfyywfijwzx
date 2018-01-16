using Exchange.BitBay;
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
using Strategy.RSI;

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
            string logFilePath = @"C:\Projects\Priv\tmp\Release\logTestApp.txt";
            ILog log = new ConsoleLogger(logFilePath);

            string x = string.Empty;

            //var krakenConfig = new ExchangeConfig();
            //krakenConfig.Load(krakenConfigPath);
            //var kraken = new Kraken(krakenConfig, 0);
            var poloniex = new Poloniex("https://poloniex.com/public", File.ReadAllText(feesJsonPoloniex), 0);

            //List<Tuple<string, string>> tradingPairs = new List<Tuple<string, string>>();
            //List<string> pairs = poloniex.GetTradablePairs();

            //foreach (var pair in pairs)
            //{
            //    var namesPair = Exchange.Poloniex.CurrenciesNamesMap.PairToCurrenciesNames(pair);

            //    if (namesPair == null)
            //        continue;

            //    tradingPairs.Add(namesPair);
            //}

            //tradingPairs = tradingPairs.Where(p => p.Item1 == Currencies.Bitcoin).ToList();

            List<Tuple<string, string>> tradingPairs = new List<Tuple<string, string>>()
            {
                //new Tuple<string, string>(Currencies.Bitcoin , Currencies.Ethereum),
                //new Tuple<string, string>(Currencies.Bitcoin , Currencies.Litecoin),
                //new Tuple<string, string>(Currencies.Bitcoin ,Currencies.BitcoinCash),
                new Tuple<string, string>(Currencies.Bitcoin ,Currencies.Ripple),
                new Tuple<string, string>(Currencies.Bitcoin ,Currencies.Litecoin)
            };


            //decimal startBalance = 1000;
            //int candleSeconds = 300;
            //TimeSpan candleInterval = new TimeSpan(0, 0, candleSeconds);
            //int ticksInCandle = 100;
            //TimeSpan tickTimeSpan = new TimeSpan(0,0,(int) candleInterval.TotalSeconds / ticksInCandle);
            //int candlesInTimeframe = 24 * 7 * 3600 / candleSeconds; //week            

            //log.Debug($"{DateTime.Now.ToString()} max candles {candlesInTimeframe}");

            //Dictionary<string, List<Candle>> historicalData = new Dictionary<string, List<Candle>>();

            //foreach(var pair in tradingPairs)
            //{
            //    log.Debug($"{DateTime.Now.ToString()} initializing market for {pair.Item1} {pair.Item2}");
            //    var hist4pairJson = poloniex.GetHistoricalData(pair.Item1, pair.Item2, DateTime.Now - new TimeSpan(7, 2, 0, 0), DateTime.Now, candleSeconds).GetAwaiter().GetResult();
            //    List<Candle> candles = JsonConvert.DeserializeObject<List<Candle>>(hist4pairJson);
            //    historicalData.Add(pair.Item1 + pair.Item2, candles.Take(candlesInTimeframe).ToList());
            //}

            //SctpBreakout breakoutStrategy = new SctpBreakout(poloniex, tradingPairs, historicalData, candleInterval, candlesInTimeframe, new decimal(0.05), startBalance, log);

            RsiStrategy rsiStrategy = new RsiStrategy(poloniex, tradingPairs,25, 14, 1800, log);

            while(true)
            {
                try
                {
                    //Task.Run(async () =>
                    //{
                    //    await rsiStrategy.Run();
                    //}).GetAwaiter().GetResult();
                    rsiStrategy.Run();
                }
                catch (Exception ex)
                {
                    log.Debug(ex.Message);
                }

                Thread.Sleep(10 *1000);
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

        string path;

        public ConsoleLogger()
        { }

        public ConsoleLogger(string path)
        {
            this.path = path;
        }

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
            if (path != null)
            {
                string msg = (string)message;
                List<string> lines = new List<string>() { msg };
                File.AppendAllLines(path, lines);
            }

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
          //  !!!
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