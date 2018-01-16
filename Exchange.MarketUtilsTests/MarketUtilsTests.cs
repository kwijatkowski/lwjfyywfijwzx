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


    namespace Exchange.MarketUtilsTests
{
    [TestFixture]
    public class MarketUtilsTests
    {

        [TestCase(OperationTypes.OPERATION_TYPE.maker, 125000, ExpectedResult = 0.0022)]
        [TestCase(OperationTypes.OPERATION_TYPE.taker, 75000, ExpectedResult = 0.0033)]
        [TestCase(OperationTypes.OPERATION_TYPE.maker, -100, ExpectedResult = 0.003)]
        [TestCase(OperationTypes.OPERATION_TYPE.taker, 0, ExpectedResult = 0.0043)]
        public decimal OrderBookReverseTest(OperationTypes.OPERATION_TYPE operationType, decimal monthlyVol)
        {
           // OrderBook ob = new OrderBook();
            return 0;
        }

        [TestCase(ExpectedResult = 48.477)]
        public decimal CalculateRSITest()
        {
            //input 
            List<decimal> closingPrices = new List<decimal>() {
                new decimal(46.1250),
                new decimal(47.1250),
                new decimal(46.4375),
                new decimal(46.9375),
                new decimal(44.9375),
                new decimal(44.25),
                new decimal(44.625),
                new decimal(45.75),
                new decimal(47.8125),
                new decimal(47.5625),
                new decimal(47),
                new decimal(44.5625),
                new decimal(46.3125),
                new decimal(47.6875),
                new decimal(46.6875)
            };
            int period = 14;

            RSI rsiCalc = new RSI();
            decimal rsi =  rsiCalc.CalculateRSI(closingPrices, period);
            return rsi;
        }
    }
}
