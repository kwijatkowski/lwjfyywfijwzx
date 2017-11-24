using System;
using System.Collections.Generic;

namespace Exchange.MarketUtils
{
    public class OrderBook
    {
        private string _currency1;
        private string _currency2;

        public string currency1 {  get { return _currency1; } }
        public string currency2 { get { return _currency2; } }

        public List<Ask> asks;
        public List<Bid> bids;


        /// <summary>
        /// OrderBook constructor
        /// </summary>
        /// <param name="c1">Plain currency name</param>
        /// <param name="c2">Plain currency name</param>
        public OrderBook(string c1, string c2)
        {
            if (string.IsNullOrWhiteSpace(c1) || string.IsNullOrWhiteSpace(c2))
                throw new Exception("Please define names for currencies");

            _currency1 = c1;
            _currency2 = c2;
            asks = new List<Ask>();
            bids = new List<Bid>();
        }

        //public decimal AskWeightAvg(string currency1, string currency2)
        public decimal AskWeightAvg()
        {
            //if (!IsPairReversed(currency1, currency2))
                return CalcWeightAverage(asks);
            //else
            //    return 1 / CalcWeightAverage(asks);
        }

        //public decimal BidWeightAvg(string currency1, string currency2)
        public decimal BidWeightAvg()
        {
            //if (!IsPairReversed(currency1, currency2))
                return CalcWeightAverage(bids);
            //else
            //    return 1 / CalcWeightAverage(bids);
        }

        //private bool IsPairReversed(string currency1, string currency2)
        //{
        //    if (string.IsNullOrWhiteSpace(_currency1) || string.IsNullOrWhiteSpace(_currency2))
        //        throw new Exception("Define currency pair for orderbook first");

        //    if (_currency1 == currency1 && _currency2 == currency2)
        //        return false;
        //    else if (_currency1 == currency2 && _currency2 == currency1)
        //        return true;
        //    else
        //        throw new Exception($"Orderbook was defined for pair {_currency1}/{_currency2} not {currency1} {currency2}");
        //}

        private static decimal CalcWeightAverage(IEnumerable<Offer> offers)
        {
            decimal sumWiXi = 0;
            decimal sumWi = 0;

            foreach (var o in offers)
            {
                sumWiXi += o.price * o.volume;
                sumWi += o.volume;
            }

            if (sumWiXi == 0)
                return 0;
            else
                return sumWiXi / sumWi;
        }

        public OrderBook Invert(OrderBook ob)
        {
            OrderBook inverted = new OrderBook(ob.currency1, ob.currency2);

            foreach (var bid in ob.bids)
                inverted.bids.Add(new Bid() { price = 1 / bid.price, timestamp = bid.timestamp, volume = bid.volume * bid.price });

            foreach (var ask in ob.asks)
                inverted.asks.Add(new Ask() { price = 1 / ask.price, timestamp = ask.timestamp, volume = ask.volume * ask.price });

            return inverted;
        }
    }
}