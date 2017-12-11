using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;

namespace Strategy.SCTP_BREAKOUT
{
    public class SctpBreakout
    {
        List<MarketToProcess> _markets; //key here is just concat of two currencies
        IExchange _exchange;
        decimal _targetProfit;

        public SctpBreakout(IExchange exchange, List<Tuple<string,string>> pairs, int candlesPeriod, decimal targetProfit)
        {
            foreach(var pair in pairs)
            {
                bool isInverted = false;
                var ordered = exchange.MakeValidPair(pair.Item1, pair.Item2, out isInverted);

                MarketToProcess market;

                if (isInverted)
                {
                    market = new MarketToProcess()
                    {
                        currency1 = ordered.Item1,
                        currency2 = ordered.Item2,
                        candles = new Queue<Candle>()
                    };
                }
                else
                {
                    market = new MarketToProcess()
                    {
                        currency1 = ordered.Item2,
                        currency2 = ordered.Item1,
                        candles = new Queue<Candle>()
                    };
                }
            }
        }

        public async void Run()
        {
            foreach (var market in _markets)
            {
                //get current ticker
                Ticker ticker = await _exchange.GetTicker(market.currency1, market.currency2);

                //get last candle
                //if ()

                //push it to queue

                //find max close price

                //check if this is new highest price
            }            
        }
    }

    //Task<Candle> GetCandle(string currency1, string currency2);


    public class MarketToProcess
    {
        public string currency1;
        public string currency2;
        public Queue<Candle> candles;
    }
    
}
