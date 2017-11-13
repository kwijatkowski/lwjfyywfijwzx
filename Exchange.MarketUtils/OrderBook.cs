using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public class OrderBook
    {
        public OrderBook()
        {
            asks = new List<Ask>();
            bids = new List<Bid>();
        }

        string currency1;
        string currency2;

        public decimal AskWeightAvg {  get { return CalcWeightAverage(asks); } }
        public decimal BidWeightAvg { get { return CalcWeightAverage(bids); } }

        public List<Ask> asks;
        public List<Bid> bids;

        private static decimal CalcWeightAverage(IEnumerable<Offer> offers)
        {
            decimal sumWiXi = 0;
            decimal sumWi = 0;

            foreach (var o in offers)
            {
                sumWiXi += o.price * o.volume;
                sumWi += o.volume;
            }

            return sumWiXi / sumWi;
        }

}
}
