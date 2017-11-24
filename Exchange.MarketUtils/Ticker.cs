using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public class Ticker
    {
        public string currency1;
        public string currency2;
        public decimal min;
        public decimal max;
        public decimal ask;
        public decimal bid;
        public decimal last;

        public Ticker Invert(Ticker t)
        {
            var inverted = new Ticker();
            inverted.currency1 = t.currency2;
            inverted.currency2 = t.currency1;

            inverted.min = 1 / t.min;
            inverted.max = 1 / t.max;
            inverted.ask = 1 / t.ask;
            inverted.bid = 1 / t.bid;
            inverted.last = 1 / t.last;

            return inverted;
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
    }

}
