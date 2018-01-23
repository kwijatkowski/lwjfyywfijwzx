using System;
using System.Collections.Generic;
using System.Linq;

namespace Exchange.MarketUtils
{
    public class RSI
    {
        public decimal CalculateRSI(List<decimal> closingPrices, int period)
        {
            if (closingPrices.Count <= period)
            {
                return 100; //just for now. Do not want to interrupt calculation for all other currencies
                //throw new Exception($"Need more data to calculate RSI. Closing prices data count {closingPrices.Count} requested period {period}");
            }

            decimal gainsTotal = 0;
            decimal losesTotal = 0;

            int startIdx = closingPrices.Count - period;

            for (int i = startIdx; i < closingPrices.Count; i++)
            {
                if (closingPrices[i] > closingPrices[i - 1])
                    gainsTotal += closingPrices[i] - closingPrices[i - 1];
                else
                    losesTotal += Math.Abs(closingPrices[i] - closingPrices[i - 1]);
            }

            decimal avgGain = gainsTotal / period;
            decimal avgLoss = losesTotal / period;

            decimal firstRS = avgGain / avgLoss;

            return 100 - 100 / (1 + firstRS);
        }

        /// <summary>
        /// Calculating rsi starting from beggining of the set, thru all candels until the end of the set.
        /// If we have 16 candles and period set to 14 we should get two values, 3 for 17 candles etc 
        /// </summary>
        /// <param name="closingPrices">set of data</param>
        /// <param name="period">period for which single rsi value is calculated</param>
        /// <returns>list with multiple rsi values for each candle after definad period (for period set to 14 for 15th candle and so on}</returns>
        public List<decimal> CalcRsiForTimePeriod(List<decimal> closingPrices, int period)
        {
            List<decimal> results = new List<decimal>();

            if (closingPrices.Count < period)
                return results;

            for (int offset = 0; offset < closingPrices.Count - period; offset++)
            {
                List<decimal> subset = new List<decimal>();

                for (int i = offset; i <= period + offset; i++)
                    subset.Add(closingPrices[i]);

                decimal rsiForSubset = CalculateRSI(subset, period);
                results.Add(rsiForSubset);
            }

            return results; 
        }        
    }
}