using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Kraken
{
    internal class KrakenOrderBook
    {
        public decimal[,] asks;
        public decimal[,] bids;
        public long timestamp;
    }
}
