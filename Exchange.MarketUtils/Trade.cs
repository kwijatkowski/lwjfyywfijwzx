using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public class Trade
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal SellPrice { get; set; }
        public decimal BuyPrice { get; set; }
        public string Curr1 { get; set; }
        public string Curr2 { get; set; }
        public int NumberOfCandles { get; set; }
    }
}
