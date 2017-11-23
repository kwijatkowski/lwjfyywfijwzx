using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public class Profit
    {
        public string currency;
        public decimal absoluteValue;
        public decimal percent;

        public static Profit NoProfit()
        {
            return new Profit()
            {
                currency = "any",
                absoluteValue = 0,
                percent = 0
            };
        }
    }

}
