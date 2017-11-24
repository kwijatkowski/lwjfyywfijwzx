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
    public class Tests
    {
        public static string publicAPIAddress = "https://poloniex.com/public";
        public static string feesScheduleJson = @"{""transfer"":[{""currency"":""any"",""incoming"":0,""outgoing"":0,""feeType"":""absolute""}],""transaction"":[{""treshold"":600,""maker"":0.0015,""taker"":0.0025,""feeType"":""percentage""},{""treshold"":1200,""maker"":0.0014,""taker"":0.0024,""feeType"":""percentage""},{""treshold"":2400,""maker"":0.0012,""taker"":0.0022,""feeType"":""percentage""},{""treshold"":6000,""maker"":0.001,""taker"":0.002,""feeType"":""percentage""},{""treshold"":12000,""maker"":0.0008,""taker"":0.0016,""feeType"":""percentage""},{""treshold"":18000,""maker"":0.0005,""taker"":0.0014,""feeType"":""percentage""},{""treshold"":24000,""maker"":0.0002,""taker"":0.0012,""feeType"":""percentage""},{""treshold"":60000,""maker"":0,""taker"":0.001,""feeType"":""percentage""},{""treshold"":120000,""maker"":0,""taker"":0.0008,""feeType"":""percentage""},{""treshold"":999999999999999,""maker"":0,""taker"":0.0005,""feeType"":""percentage""}]}";
        public static decimal accountMonthlyVolume = 0;

        public Tests()
        {
            System.Net.WebRequest.DefaultWebProxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
        }

        //[TestCase]
        //public void CurrenciesMapTest()
        //{
        //    Exchange.Poloniex.Poloniex p = new Exchange.Poloniex.Poloniex(publicAPIAddress, feesScheduleJson, accountMonthlyVolume);
        //    Dictionary<string, string> map = p.GetCurrenciesMap();
        //    Assert.IsNotNull(map);
        //}

        [TestCase]
        public void GetTradablePairs()
        {
            Exchange.Poloniex.Poloniex p = new Exchange.Poloniex.Poloniex(publicAPIAddress, feesScheduleJson, accountMonthlyVolume);
            List<string> pairs = p.GetTradablePairs();
            Assert.IsNotNull(pairs);
        }

        [TestCase(OperationTypes.OPERATION_TYPE.maker, 3, ExpectedResult = 0.0015)]
        [TestCase(OperationTypes.OPERATION_TYPE.taker, 700, ExpectedResult = 0.0024)]
        [TestCase(OperationTypes.OPERATION_TYPE.maker, 1600, ExpectedResult = 0.0012)]
        [TestCase(OperationTypes.OPERATION_TYPE.taker, 2500, ExpectedResult = 0.002)]
        public decimal TransactionFees(OperationTypes.OPERATION_TYPE operationType, decimal monthlyVol)
        {
            OperationCostCalculator calc = new OperationCostCalculator(feesScheduleJson, monthlyVol);

            decimal result = calc.CalculateTransactionFee("", operationType);
            return result;

        }

        [TestCase("BTC", OperationTypes.TRANSFER_DIR.incomming, ExpectedResult = 0)]
        [TestCase("BTC", OperationTypes.TRANSFER_DIR.outgoing, ExpectedResult = 0)]
        [TestCase("ETH", OperationTypes.TRANSFER_DIR.incomming, ExpectedResult = 0)]
        [TestCase("ETH", OperationTypes.TRANSFER_DIR.outgoing, ExpectedResult = 0)]
        public decimal TransferFees(string currency, OperationTypes.TRANSFER_DIR direction, decimal monthlyVol)
        {
            OperationCostCalculator calc = new OperationCostCalculator(feesScheduleJson, monthlyVol);

            decimal result = calc.CalculateTransferCost(currency, direction);
            return result;
        }
    }
}

