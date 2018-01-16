using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;
using log4net;

namespace Strategy.SCTP_BREAKOUT
{
    public class SctpBreakout
    {
        List<MarketToProcess> _markets; //key here is just concat of two currencies
        IExchange _exchange;
        decimal _targetProfit;
        int _candlesCount;
        
        TimeSpan _candleInterval;

        //decimal currentBalance;
        //decimal prevBalance;
        //decimal positionBalance;

        decimal range = new decimal(0.03); //treshold - of price peak not within this boundary, then we missed the moment and do not want ot buy now
        ILog logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="pairs"></param>
        /// <param name="candleInterval">candle interval</param>
        /// <param name="candlesInWindow">Candles in considered time window</param>
        /// <param name="targetProfit"></param>
        /// <param name="logger"></param>
        public SctpBreakout(IExchange exchange, List<Tuple<string,string>> pairs, Dictionary<string,List<Candle>> marketInitializationData, TimeSpan candleInterval, int candlesInWindow, decimal targetProfit, decimal startBalance, ILog logger)
        {            
            _exchange = exchange;
            _candlesCount = candlesInWindow;
            _targetProfit = targetProfit;
            _candleInterval = candleInterval;
            _markets = new List<MarketToProcess>();
            this.logger = logger;

            foreach(var pair in pairs)
            {
                bool isInverted = false;
                var ordered = exchange.MakeValidPair(pair.Item1, pair.Item2, out isInverted);

                List<Candle> marketInitializationCandles = null;

                if (marketInitializationData != null)
                marketInitializationData.TryGetValue(string.Concat(ordered.Item1, ordered.Item2), out marketInitializationCandles);

                MarketToProcess market
                = new MarketToProcess(
                    ordered.Item1,
                    ordered.Item2,
                    _candlesCount,
                    startBalance / pairs.Count, //equally for the markets
                    marketInitializationCandles);                       

                _markets.Add(market);
            }
        }

        /// <summary>
        /// This method is executing stratego iteratrion. Execute is as often as you want to collect tickers for candles, 
        /// </summary>
        public async Task Run()
        {
            DateTime now = DateTime.Now;

            foreach (var market in _markets)
            {
                if (!market.IsInitialized())
                {
                    logger.Debug($"{DateTime.Now.ToString()} {market.currency1} {market.currency2} Market not initialized yet. Candles {market.candles.Count}, ticks {market.lastCandleTickers.Count}");
                }

                //get current ticker
                Ticker ticker = await _exchange.GetTicker(market.currency1, market.currency2);

                Run(now, _candleInterval, market, ticker);
            }
        }

        public void Run(DateTime now, TimeSpan interval, MarketToProcess market, Ticker ticker)
        {
            if (now - interval >= market.lastCandleClosedTime) // need to close candle
            {
                market.PushCandle(TickersToCandle(market.lastCandleTickers));
                market.lastCandleClosedTime = DateTime.Now;
                //logger.Debug($"{DateTime.Now.ToString()} {market.currency1} {market.currency2} Candle pushed, now {market.candles.Count}");
                market.CleanTickers();
            }
            else
            {
                market.lastCandleTickers.Add(ticker);
            }

            //is ticker price new highest price in timeframe we analyze?
            if (market.timeframeHighPrice != null && ticker.last > market.timeframeHighPrice)
            {
                //market.timeframeHighPrice = ticker.last;
                logger.Debug($"{now.ToString()} {market.currency1} {market.currency2} NEW!!!!!!! highest price {market.timeframeHighPrice} in timeframe");
            }
            else if (market.timeframeHighPrice != null)
            {
                //logger.Debug($"{now.ToString()} {market.currency1} {market.currency2} timeframe highest {market.timeframeHighPrice}");
            }

            ProcessBuySell(market, ticker);
        }

        public void ProcessBuySell(MarketToProcess market, Ticker ticker)
        {
            if (ShouldBuy(market, ticker))
            {
                logger.Debug($" BUY!!!!!!!!!!!!!!!!!!!!!!!!!!!! {market.currency1} {market.currency2} bougth at {ticker.last}");
                ///FAKE
                market.buyPrice = ticker.last;
                market.sellPrice  = ticker.last * (1 + _targetProfit);
                market.availableBalance = market.availableBalance / ticker.last;
                market.previousBalance = market.availableBalance;
                ///FAKE

                market.status = MarketToProcess.STATUS.IN_POSITION;
            }

            //if (_exchange.BalanceForPair(market.currency1, market.currency2) != 0)

            if (ShouldSell(market, ticker))
            {
                logger.Debug($" SELL!!!!!!!!!!!!!!!!!!!!!!!!!!!! {market.currency1} {market.currency2} sold at {ticker.last}");

                ///FAKE
                market.availableBalance = market.availableBalance * ticker.last;
                ///FAKE
                ///
                logger.Debug($" {market.currency1} {market.currency2} profit {market.availableBalance - market.previousBalance}");
                market.status = MarketToProcess.STATUS.OPEN;

                if (market.availableBalance <= market.previousBalance)
                {
                    logger.Debug($" {market.currency1} {market.currency2} ERROR {market.availableBalance - market.previousBalance}");
                    //logger.Debug($" {market.currency1} {market.currency2} profit {currentBalance - prevBalance}");
                    //   throw new Exception("something went wrong - we are making loses");
                }                
            }
        }

        private bool ValueWithinRange(decimal actualValue, decimal desiredValue)
        {
            if (
                actualValue < desiredValue * (1 + range) &&
                actualValue > desiredValue * (1 - range))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Candle TickersToCandle(List<Ticker> tickers)
        {
            var open = tickers.First().last;
            var high = tickers.Max(t => t.last);
            var close = tickers.Last().last;
            var low = tickers.Min(t => t.last);

            return new Candle(open, high, close, low);
        }

        public bool ShouldBuy(MarketToProcess market, Ticker t)
        {
            if (!market.IsInitialized() || market.status != MarketToProcess.STATUS.OPEN)
                return false;

            //check for candle separation and boilinger bands

            if (market.timeframeHighPrice <= t.last && t.last < market.timeframeHighPrice * (1+range) )
                return true;
            else
                return false;                
        }

        public bool ShouldSell(MarketToProcess market, Ticker t)
        {
            if (!market.IsInitialized() || market.status != MarketToProcess.STATUS.IN_POSITION)
                return false;           

            if (market.sellPrice <= t.last)
                return true;
            else
                return false;
        }
    }

    //Task<Candle> GetCandle(string currency1, string currency2);

    public class MarketToProcess
    {
        public enum STATUS { OPEN, BUYING, IN_POSITION, SELLING };

        public string currency1;
        public string currency2;
        public decimal availableBalance;
        public decimal previousBalance;
        public Queue<Candle> candles;
        public List<Ticker> lastCandleTickers;        
        private int _timeframeCandlesCount;
        public DateTime lastCandleClosedTime;

        public decimal buyingVolume;
        public decimal? timeframeHighPrice;
        public decimal? buyPrice;
        public decimal? sellPrice;
        public STATUS status;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="timeframeCandlesCount">This is as number of candles which will be kept and analyzed</param>
        /// <param name="historicalData"></param>
        public MarketToProcess(string currency1, string currency2, int timeframeCandlesCount, decimal availabeBalance, List<Candle> historicalData = null)
        {
            availableBalance = availabeBalance;
            lastCandleClosedTime = DateTime.Now;
            _timeframeCandlesCount = timeframeCandlesCount;
            this.currency1 = currency1;
            this.currency2 = currency2;
            candles = historicalData == null ? new Queue<Candle>() : new Queue<Candle>(historicalData.Take(timeframeCandlesCount));

            if (candles != null)
                timeframeHighPrice = candles.Max(c => c.Close);
            else
                timeframeHighPrice = null;

            lastCandleTickers = new List<Ticker>();
        }

        /// <summary>
        /// Checking if we already have full set of candles for timeframe
        /// </summary>
        /// <returns></returns>
        public bool IsInitialized()
        {
            if (candles.Count == _timeframeCandlesCount)
                return true;
            else
                return false;
        }

        public void SetNewHigh(decimal newHigh)
        {
            timeframeHighPrice = newHigh;
        }

        public void PushCandle(Candle candle)
        {
            candles.Enqueue(candle);

            if (candles.Count > _timeframeCandlesCount)
                candles.Dequeue();

            timeframeHighPrice = candles.Max(c => c.Close);
        }

        public void CleanTickers()
        {
            lastCandleTickers = new List<Ticker>();
        }
    }
    
}
