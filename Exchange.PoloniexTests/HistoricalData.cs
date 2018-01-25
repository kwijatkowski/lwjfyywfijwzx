using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Exchange.MarketUtils;
using Exchange.Poloniex;

namespace Exchange.PoloniexTests
{

        [TestFixture]
        public class DataForTests
        {

        public static string publicAPIAddress = "https://poloniex.com/public";
        public static string feesScheduleJson = @"{""transfer"":[{""currency"":""any"",""incoming"":0,""outgoing"":0,""feeType"":""absolute""}],""transaction"":[{""treshold"":600,""maker"":0.0015,""taker"":0.0025,""feeType"":""percentage""},{""treshold"":1200,""maker"":0.0014,""taker"":0.0024,""feeType"":""percentage""},{""treshold"":2400,""maker"":0.0012,""taker"":0.0022,""feeType"":""percentage""},{""treshold"":6000,""maker"":0.001,""taker"":0.002,""feeType"":""percentage""},{""treshold"":12000,""maker"":0.0008,""taker"":0.0016,""feeType"":""percentage""},{""treshold"":18000,""maker"":0.0005,""taker"":0.0014,""feeType"":""percentage""},{""treshold"":24000,""maker"":0.0002,""taker"":0.0012,""feeType"":""percentage""},{""treshold"":60000,""maker"":0,""taker"":0.001,""feeType"":""percentage""},{""treshold"":120000,""maker"":0,""taker"":0.0008,""feeType"":""percentage""},{""treshold"":999999999999999,""maker"":0,""taker"":0.0005,""feeType"":""percentage""}]}";
        public static decimal accountMonthlyVolume = 0;
        public static string resultsJsonDir = @"C:\temp\test";
        public static DateTime startDate = new DateTime(2017, 1, 1, 0, 0, 0);
        public static DateTime endDate = new DateTime(2017, 12, 31, 23, 59, 59);
        public static int candlePeriod = 1800;

        public DataForTests()
        {
            System.Net.WebRequest.DefaultWebProxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
        }


        [TestCase]
        public void DownloadHistoricalData()
        {
            Exchange.Poloniex.Poloniex poloniex = new Exchange.Poloniex.Poloniex(publicAPIAddress, feesScheduleJson, accountMonthlyVolume);

            //get tradable pairs
            List<Tuple<string, string>> tradingPairs = new List<Tuple<string, string>>();
            List<string> pairs = poloniex.GetTradablePairs();

            foreach (var pair in pairs)
            {
                var namesPair = Exchange.Poloniex.CurrenciesNamesMap.PairToCurrenciesNames(pair);

                if (namesPair == null)
                    continue;

                tradingPairs.Add(namesPair);
            }

            tradingPairs = tradingPairs.Where(p => p.Item1 == Currencies.Bitcoin).ToList();

            foreach (var pair in tradingPairs)
            {
                var singleCryptoResult = poloniex.GetHistoricalData(pair, startDate, endDate, candlePeriod).GetAwaiter().GetResult();
                File.WriteAllText(Path.Combine(resultsJsonDir, $"{pair.Item1}_{pair.Item2}_2017_1800.json"),singleCryptoResult.Item2);
            }
        }

        }
}
