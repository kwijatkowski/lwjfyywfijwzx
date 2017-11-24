﻿using Exchange.MarketUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Strategy.Arbitrage
{
    public class ArbitrageStrategy
    {
        private int _orderbookCountLimit;
        private decimal _maxOrderbookPriceDeviation;

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderbookCountLimit"></param>
        /// <param name="maxOrderbookPriceDeviation">1 == 100%</param>
        public ArbitrageStrategy(int? orderbookCountLimit, decimal maxOrderbookPriceDeviation)
        {
            if (orderbookCountLimit == null)
                orderbookCountLimit = int.MaxValue;
            else
                _orderbookCountLimit = (int) orderbookCountLimit;

            _maxOrderbookPriceDeviation = maxOrderbookPriceDeviation;
        }

        /// <summary>
        /// We assume we buy at start exchange and sell at target exchange
        /// </summary>
        /// <param name="startEndCurrency">Currency which we are buying with. Must be not mapped</param>
        /// <param name="tmpCurrency">Currency which we ware buying. Must be not mapped</param>
        /// <param name="startExchange"></param>
        /// <param name="targetExchange"></param>
        /// <param name="startEndCurrencyVolume"></param>
        /// <returns></returns>
        public async Task<Profit> CalculateSingleTransferProfitForPairAndExchange(string startEndCurrency, string tmpCurrency, IExchange startExchange, IExchange targetExchange, decimal startEndCurrencyVolume)
        {
            bool startInverted = false;
            bool targetInverted = false;
            Tuple<string, string> startExchangePair = startExchange.MakeValidPair(startEndCurrency, tmpCurrency, out startInverted);
            if (startExchangePair == null)
            {
                Console.WriteLine($"{startEndCurrency} and {tmpCurrency} is not valid pair for {startExchange.GetName()}");
                return Profit.NoProfit();
            }
            //throw new ArgumentException($"{startEndCurrency} and {tmpCurrency} is not valid pair for {startExchange.GetName()}");

            Tuple<string, string> targetExchangePair = targetExchange.MakeValidPair(startEndCurrency, tmpCurrency, out targetInverted);
            if (targetExchangePair == null)
            {
                Console.WriteLine($"{startEndCurrency} and {tmpCurrency} is not valid pair for {targetExchange.GetName()}");
                return Profit.NoProfit();
            }
            //throw new ArgumentException($"{startEndCurrency} and {tmpCurrency} is not valid pair for {targetExchange.GetName()}");

            OrderBook startOrderbook = null;
            OrderBook targetOrderbook = null;
            
            try
            {
                 startOrderbook = await GetLimitedOrderbook(startExchange, startEndCurrency, tmpCurrency, _orderbookCountLimit);
                 targetOrderbook = await GetLimitedOrderbook(targetExchange, tmpCurrency, startEndCurrency, _orderbookCountLimit); //must be inverted
            }
            catch(Exception ex)
            {
                return Profit.NoProfit();
            }
            //if there are no offers, then return profit 0
            if (!startOrderbook.bids.Any() || !targetOrderbook.asks.Any())
            {
                Console.WriteLine($"No bids or asks in orderbook");
                return Profit.NoProfit();
            }
            else
            {
                decimal tmpVolume = startEndCurrencyVolume;

                //buy fee %
                decimal tsf1 = startExchange.CalculateTransacionFee(startEndCurrency, tmpCurrency);
                tmpVolume *= (1 - tsf1); //volume after fee

                //recalculate to tmpCurrency
                tmpVolume = tmpVolume * startOrderbook.BidWeightAvg(); //!!! make sure it is inverted if needed

                //transfer fee
                decimal trf1 = startExchange.CalculateTransferFee(tmpCurrency, tmpVolume);
                tmpVolume -= trf1; //this is always 

                //to do: make sure deposit fee is always 0

                //sell fee %
                decimal tsf2 = targetExchange.CalculateTransacionFee(tmpCurrency, startEndCurrency);
                tmpVolume *= (1 - tsf2); //volume after fee

                //recalculate to startTargetCurrency
                tmpVolume = tmpVolume * targetOrderbook.AskWeightAvg(); //!!! make sure it is inverted if needed

                //todo: take fees into account
                Profit profit = new Profit()
                {
                    percent = (tmpVolume - startEndCurrencyVolume) / startEndCurrencyVolume * 100,
                    absoluteValue = tmpVolume - startEndCurrencyVolume,
                    currency = startEndCurrency
                };

                return profit;
            }
        }

        private async Task<OrderBook> GetLimitedOrderbook(IExchange exchange, string currency1, string currency2, int countLimit)
        {           
            var startTicker = await exchange.GetTicker(currency1, currency2);

            var bidLimit = (1 - _maxOrderbookPriceDeviation) * startTicker.last; //number is percentage variation of price which we want to allow
            var askLimit = (1 + _maxOrderbookPriceDeviation) * startTicker.last; //number is percentage variation of price which we want to allow

            var orderBook = await exchange.GetOrderbook(currency1, currency2, countLimit);
            orderBook.asks = orderBook.asks.Where(a => bidLimit < a.price && a.price < askLimit).ToList().Take(countLimit).ToList();
            orderBook.bids = orderBook.bids.Where(b => askLimit > b.price && b.price > bidLimit).ToList().Take(countLimit).ToList();

            return orderBook;
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