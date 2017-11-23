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
    }
}
