using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
        public class Candle
        {

            public decimal Open { get; }
            public decimal High { get; }
            public decimal Close { get; }
            public decimal Low { get; }

            public Candle(decimal open, decimal high, decimal close, decimal low)
            {
                Open = open;
                High = high;
                Close = close;
                Low = low;
            }

            public bool IsTheSame(Candle toCompare)
            {
                if (
                    this.Open != toCompare.Open ||
                    this.High != toCompare.High ||
                    this.Close != toCompare.Close ||
                    this.Low != toCompare.Low)
                {
                    return false;
                }
                else
                    return true;
            }
        }
}
