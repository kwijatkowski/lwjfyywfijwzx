using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using Exchange.BitBay;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Exchange.MarketUtils;

namespace Exchange.BitBayTests
{
    [TestFixture]
    public class FeesTests
    {
        public string feesJson = @"{""transfer"":[{""currency"":""BTC"",""incoming"":0,""outgoing"":0.00045,""feeType"":""absolute""},{""currency"":""BCC"",""incoming"":0,""outgoing"":0.0006,""feeType"":""absolute""},{""currency"":""ETH"",""incoming"":0,""outgoing"":0.00126,""feeType"":""absolute""},{""currency"":""LTC"",""incoming"":0,""outgoing"":0.005,""feeType"":""absolute""},{""currency"":""LSK"",""incoming"":0,""outgoing"":0.2,""feeType"":""absolute""},{""currency"":""Dash"",""incoming"":0,""outgoing"":0.001,""feeType"":""absolute""},{""currency"":""GAME"",""incoming"":0,""outgoing"":0.005,""feeType"":""absolute""}],""transaction"":[{""treshold"":1250,""tresholdCurrency"":""EUR"",""maker"":0.003,""taker"":0.0043,""feeType"":""percentage""},{""treshold"":3750,""tresholdCurrency"":""EUR"",""maker"":0.0029,""taker"":0.0042,""feeType"":""percentage""},{""treshold"":7500,""tresholdCurrency"":""EUR"",""maker"":0.0028,""taker"":0.0041,""feeType"":""percentage""},{""treshold"":10000,""tresholdCurrency"":""EUR"",""maker"":0.0028,""taker"":0.0040,""feeType"":""percentage""},{""treshold"":15000,""tresholdCurrency"":""EUR"",""maker"":0.0027,""taker"":0.0039,""feeType"":""percentage""},{""treshold"":20000,""tresholdCurrency"":""EUR"",""maker"":0.0026,""taker"":0.0038,""feeType"":""percentage""},{""treshold"":25000,""tresholdCurrency"":""EUR"",""maker"":0.0025,""taker"":0.0037,""feeType"":""percentage""},{""treshold"":37500,""tresholdCurrency"":""EUR"",""maker"":0.0025,""taker"":0.0036,""feeType"":""percentage""},{""treshold"":50000,""tresholdCurrency"":""EUR"",""maker"":0.0024,""taker"":0.0035,""feeType"":""percentage""},{""treshold"":75000,""tresholdCurrency"":""EUR"",""maker"":0.0023,""taker"":0.0034,""feeType"":""percentage""},{""treshold"":100000,""tresholdCurrency"":""EUR"",""maker"":0.0023,""taker"":0.0033,""feeType"":""percentage""},{""treshold"":150000,""tresholdCurrency"":""EUR"",""maker"":0.0022,""taker"":0.0032,""feeType"":""percentage""},{""treshold"":200000,""tresholdCurrency"":""EUR"",""maker"":0.0021,""taker"":0.0031,""feeType"":""percentage""},{""treshold"":250000,""tresholdCurrency"":""EUR"",""maker"":0.0021,""taker"":0.003,""feeType"":""percentage""},{""treshold"":375000,""tresholdCurrency"":""EUR"",""maker"":0.002,""taker"":0.0029,""feeType"":""percentage""},{""treshold"":500000,""tresholdCurrency"":""EUR"",""maker"":0.0019,""taker"":0.0028,""feeType"":""percentage""},{""treshold"":625000,""tresholdCurrency"":""EUR"",""maker"":0.0018,""taker"":0.0027,""feeType"":""percentage""},{""treshold"":875000,""tresholdCurrency"":""EUR"",""maker"":0.0018,""taker"":0.0026,""feeType"":""percentage""},{""treshold"":1000000000,""tresholdCurrency"":""EUR"",""maker"":0.0017,""taker"":0.0025,""feeType"":""percentage""}]}";

        [TestCase(OperationTypes.OPERATION_TYPE.maker , 125000, ExpectedResult = 0.0022)]
        [TestCase(OperationTypes.OPERATION_TYPE.taker , 75000, ExpectedResult = 0.0033)]
        [TestCase(OperationTypes.OPERATION_TYPE.maker , -100, ExpectedResult = 0.003)]
        [TestCase(OperationTypes.OPERATION_TYPE.taker , 0, ExpectedResult = 0.0043)]
        public decimal TransactionFees(OperationTypes.OPERATION_TYPE operationType, decimal volume)
        {
            OperationCostCalculator calc = new OperationCostCalculator(feesJson);

            decimal result = calc.CalculateTransactionFee("",operationType, volume);
            return result;

        }

        [TestCase("BTC", OperationTypes.TRANSFER_DIR.incomming, ExpectedResult = 0)]
        [TestCase("BTC", OperationTypes.TRANSFER_DIR.outgoing, ExpectedResult = 0.00045)]
        [TestCase("ETH", OperationTypes.TRANSFER_DIR.incomming, ExpectedResult = 0)]
        [TestCase("ETH", OperationTypes.TRANSFER_DIR.outgoing, ExpectedResult = 0.00126)]
        public decimal TransferFees(string currency, OperationTypes.TRANSFER_DIR direction)
        {
            OperationCostCalculator calc = new OperationCostCalculator(feesJson);

            decimal result =  calc.CalculateTransferCost(currency, direction);
            return result;
        }
    }
}
