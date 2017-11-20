using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;

namespace Strategy.Arbitrage
{
    public class ArbitrageStrategy
    {
        int? _orderbookCountLimit;

        public ArbitrageStrategy(int? orderbookCountLimit)
        {
            _orderbookCountLimit = orderbookCountLimit;
        }

        // public List<string> FindMostProfitableTransferPair(IExchange startExchange, List<IExchange> allExchanges)
        // {


        //  }

            /// <summary>
            /// We assume we buy at start exchange and sell at target exchange
            /// </summary>
            /// <param name="startEndCurrency">Currency which we have and want to buy some other currency with. Not mapped</param>
            /// <param name="transferCurrency">Currency which we want to make profit from. Not mapped</param>
            /// <param name="startExchange"></param>
            /// <param name="targetExchange"></param>
            /// <returns></returns>
        public async Task<Profit> CalculateProfitForPairAndExchange(string startEndCurrency, string transferCurrency, IExchange startExchange, IExchange targetExchange)
        {
            if (!startExchange.IsValidPair(startEndCurrency, transferCurrency))
                throw new ArgumentException($"{startEndCurrency} and {transferCurrency} is not valid pair for {startExchange.GetName()}");

            if (!targetExchange.IsValidPair(startEndCurrency, transferCurrency))
                throw new ArgumentException($"{startEndCurrency} and {transferCurrency} is not valid pair for {targetExchange.GetName()}");

            var startOrderbook = await GetLimitedOrderbook(startExchange, startEndCurrency, transferCurrency);
            var targetOrderbook = await GetLimitedOrderbook(targetExchange, startEndCurrency, transferCurrency);

            //todo: take fees into account
            Profit profit = new Profit()
            {
                percent = targetOrderbook.AskWeightAvg / startOrderbook.BidWeightAvg,
                absoluteValue = targetOrderbook.AskWeightAvg - startOrderbook.BidWeightAvg,
                currency = startEndCurrency
            };

            return profit;
        }

        private async Task<OrderBook> GetLimitedOrderbook(IExchange exchange, string currency1, string currency2, int? countLimit = null)
        {
            var startTicker = await exchange.GetTicker(currency1, currency2);

            var startBidLimit = new decimal(0.7) * startTicker.last; //number is percentage variation of price which we want to allow
            var startAskLimit = new decimal(1.3) * startTicker.last; //number is percentage variation of price which we want to allow

            return await exchange.GetOrderbook(currency1, currency2, startBidLimit, startAskLimit, countLimit);
        }



        public List<Tuple<string, string, string, decimal, decimal>> CalculatePriceDifference(List<TickerListItem> tickerList)
        {
            var percentage = new List<Tuple<string, string, string, decimal, decimal>>();

            var currencies = tickerList.Select(i => i.currency1).Distinct().ToList();

            foreach (string currency in currencies)
            {
                var thisCurrencyTickers = tickerList.Where(i => i.currency1 == currency).ToList();

                for (int i = 0; i < thisCurrencyTickers.Count(); i++)
                    for (int j = i + 1; j < thisCurrencyTickers.Count(); j++)
                    {
                        var ex1Item = thisCurrencyTickers[i];
                        var ex2Item = thisCurrencyTickers[j];

                        decimal l1 = ex1Item.ticker.last;
                        decimal l2 = ex2Item.ticker.last;

                        percentage.Add(new Tuple<string, string, string, decimal, decimal>(
                            ex1Item.Exchange,
                            ex2Item.Exchange,
                            currency,
                            Math.Abs(l1 - l2) / Math.Min(l1, l2) * new decimal(100),
                            Math.Abs(ex1Item.ticker.last - ex2Item.ticker.last)));
                    }
            }
            return percentage.OrderByDescending(i => i.Item4).ToList();
        }
    }
}
