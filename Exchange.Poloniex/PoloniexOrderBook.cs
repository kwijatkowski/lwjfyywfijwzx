using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Poloniex
{
    internal class PoloniexOrderBook
    {
        public List<object[]> asks;
        public List<object[]> bids;
        public int isFrozen;
        public string seq;
    }
}
