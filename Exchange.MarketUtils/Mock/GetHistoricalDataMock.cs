using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils.Mock
{
    public static class GetHistoricalDataMock
    {
        private static Dictionary<string, List<Candle>> staticDicitionary;
        public static string LastPair;
        public static int NumberOfCandles = 0;

        static GetHistoricalDataMock()
        {
            staticDicitionary = new Dictionary<string, List<Candle>>(StringComparer.OrdinalIgnoreCase);
        }

        public static string GetHistoricalData(string pair, int numberOfCandles)
        {
            var candlesToReturn = new List<Candle>();
            List<Candle> candles;

            if (staticDicitionary.ContainsKey(pair))
            {
                candles = staticDicitionary[pair];
            }
            else
            {
                var historyString = SerializeUtility.DeSerializeObject<string>($"{pair}.xml");
                candles = JsonConvert.DeserializeObject<List<Candle>>(historyString);
                var orderByDate = candles.OrderBy(c => c.Date).ToList();
                staticDicitionary[pair] = orderByDate;
            }

            for(int i = 0; i < numberOfCandles; ++i)
            {
                if (candles.Count > 0)
                {
                    candlesToReturn.Add(candles[0]);
                    candles.RemoveAt(0);
                }                
            }

            //trochę słabe i trzeba pomyśleć...
            //żeby wszystkie dzienniki miały taką samą ilość danych
            if(numberOfCandles == 1)
            {
                foreach(var cur in staticDicitionary)
                {
                    if (!pair.Equals(cur.Key, StringComparison.OrdinalIgnoreCase) && cur.Value != null && cur.Value.Count > 0)
                        cur.Value.RemoveAt(0);
                }

                //do liczenia ostatnich swiec
                if (string.IsNullOrEmpty(LastPair))
                {
                    LastPair = pair;
                    NumberOfCandles = 1;
                }
                else
                {
                    ++NumberOfCandles;
                }
            }

            /*if(candlesToReturn.Count < numberOfCandles)
            {
                string x = "";
                x += "";
            }*/
               
            var ser = JsonConvert.SerializeObject(candlesToReturn);
            return ser;
        }
    }
}
