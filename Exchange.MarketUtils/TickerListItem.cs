using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public class TickerListItem
    {
        public string Exchange { get; set; }
        public string currency1 { get; set; }
        public string currency2 { get; set; }
        public Ticker ticker { get; set; }
    }
}
