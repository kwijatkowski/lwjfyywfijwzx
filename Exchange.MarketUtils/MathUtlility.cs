using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public static  class MathUtlility
    {
        public static decimal Median(List<decimal> xs)
        {
            var ordered = xs.OrderBy(x => x).ToList();
            double midPoint = (ordered.Count - 1) / 2.0;
            return (ordered[(int)(midPoint)] + ordered[(int)(midPoint + 0.5)]) / 2;
        }

        public static List<decimal> GetTopValues(List<decimal> ls, int count)
        {
            return ls.OrderByDescending(d => d).Take(count).ToList();
        }
    }
}
