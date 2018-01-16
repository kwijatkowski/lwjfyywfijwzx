using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public class RSI
    {
        public decimal CalculateRSI(List<decimal> closingPrices, int period)
        {
            if (closingPrices.Count <= period)
                throw new Exception($"Need more data to calculate RSI. Closing prices data count {closingPrices.Count} requested period {period}");

            decimal gainsTotal = 0;
            decimal losesTotal = 0;

            int startIdx = closingPrices.Count - period;

            for (int i = startIdx; i < closingPrices.Count; i++)
            {
                if (closingPrices[i] > closingPrices[i - 1])
                    gainsTotal += closingPrices[i] - closingPrices[i - 1];
                else
                    losesTotal += closingPrices[i - 1] - closingPrices[i];
            }

            decimal avgGain = gainsTotal / period;
            decimal avgLoss = losesTotal / period;

            decimal firstRS = avgGain / avgLoss;

            return 100 - 100 / (1 + firstRS);
        }

    }
}
