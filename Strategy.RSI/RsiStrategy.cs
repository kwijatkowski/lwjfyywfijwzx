using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;
using Exchange.Poloniex;
using log4net;
using Newtonsoft.Json;
using System.Threading;
using Exchange.Mock;

namespace Strategy.RSI
{
    public class RsiStrategy
    {
        private ITimeProvider _timeProvider;

        private enum STATE { LOOKING_FOR_OPPORTUNITY, BUYING, IN_POSITION, SELLING }
        private Queue<string> _alreadyTraded;
        private Poloniex _exchange;
        private List<Tuple<string, string>> _currenciesToWorkOn;
        private decimal _buyTreshold;
        private int _period;
        private int _candlePeriod;
        private ILog _logger;
        private STATE currentState;

        private decimal _currentBalance;

        private decimal buyPrice;
        private decimal sellPrice;
        private decimal _targetProfitPercentage;
        private Tuple<string, string> bestPair;       

        private decimal volumeTreshold;

        private decimal stopLossPrice;
        private DateTime failTime;

        //for test history 
        public LinkedList<Trade> tradeBook = new LinkedList<Trade>();
        public int trick = 0;

        public RsiStrategy(Poloniex exchange, List<Tuple<string,string>> currenciesToWorkOn, decimal rsiBuyTreshold, int period, int candlePeriod, decimal targetProfitPercentage, decimal startBalance, ILog logger, ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _exchange = exchange;
            _currenciesToWorkOn = currenciesToWorkOn;
            _buyTreshold = rsiBuyTreshold;
            _period = period;
            _candlePeriod = candlePeriod;
            _logger = logger;
            _targetProfitPercentage = targetProfitPercentage;
            _currentBalance = startBalance;
            currentState = STATE.LOOKING_FOR_OPPORTUNITY;            
        }

        public async Task Run()
        {            
            //DateTime startDate = DateTime.UtcNow - new TimeSpan(0, 0, (_period + 1) * _candlePeriod);

            if (STATE.LOOKING_FOR_OPPORTUNITY == currentState)
            {
                //implement volume treshold - take it from the poloniex public api connector public async Task<string> Get24hVolume()
                //bestPair = await FindLowestRsiPair(end);
                //volumeTreshold = await _exchange.GetVolumeThreshold(bestPair.Item1, bestPair.Item2);

                int additionalRsiPoints = 4; // one is calculated by default, so we will get additionalRsiPoints + 1
                DateTime startDate = _timeProvider.Now() - new TimeSpan(0, 0, (_period + 1 + additionalRsiPoints) * _candlePeriod);
                               
                var pairsResults = await CalculateMultiplRsiPoints(startDate, _timeProvider.Now());

            var bestPair = SelectMostPromisingPair(pairsResults);

                if (bestPair != null)
                {
                    Ticker t = _exchange.GetTicker(bestPair.Item1, bestPair.Item2).GetAwaiter().GetResult();

                    currentState = STATE.BUYING;
                    buyPrice = t.ask;
                    sellPrice = Math.Round(buyPrice * (1 + _targetProfitPercentage),8); //1 sat precision

                    var vThreshold = Math.Round(_currentBalance >= volumeTreshold ? volumeTreshold : _currentBalance, 8);
                    SetBuyOrder(bestPair.Item1, bestPair.Item2, t.ask, vThreshold);
                    _logger.Debug($"{_timeProvider.Now()} Buy order set {bestPair.Item1} {bestPair.Item2} price {t.ask} volume {vThreshold}");

                    failTime = _timeProvider.Now() + new TimeSpan(48, 0, 0);
                    stopLossPrice = buyPrice * 0.1m;

                    if (! await IsBuyOrderFilled(bestPair.Item1, bestPair.Item2))
                    {
                        //  await Task.Delay(exchangeOperationsCheckDelay);
                        return;
                    }

                    currentState = STATE.IN_POSITION;
                    _logger.Debug($"{_timeProvider.Now()} Buy order filled {bestPair.Item1} {bestPair.Item2} price {t.ask}");

                    tradeBook.AddLast(new Trade
                    {
                        Date = _timeProvider.Now(),
                        Amount = vThreshold,
                        BuyPrice = t.ask,
                        Curr1 = bestPair.Item1,
                        Curr2 = bestPair.Item2
                    });

                    //sell
                    SetSellOrder(bestPair.Item1, bestPair.Item2, sellPrice);
                    _logger.Debug($"{_timeProvider.Now()} Sell order set {bestPair.Item1} {bestPair.Item2} price {sellPrice}");
                    //currentState = STATE.SELLING;
                }
            }

            else if (currentState == STATE.IN_POSITION)
            {
                decimal tickerPrice = _exchange.GetTicker(bestPair.Item1, bestPair.Item2).GetAwaiter().GetResult().last;

                if (!NeedToSell(bestPair.Item1, bestPair.Item2, tickerPrice))
                {
                    return;
                }
                else
                {
                    tradeBook.Last().SellPrice = tickerPrice;
                    //tradeBook.Last().NumberOfCandles = GetHistoricalDataMock.NumberOfCandles;
                    //GetHistoricalDataMock.LastPair = null;

                    _logger.Debug($"{_timeProvider.Now()} Sell order filled {bestPair.Item1} {bestPair.Item2} price {tickerPrice}");
                    _logger.Debug($"Sold {bestPair.Item1} {bestPair.Item2} @ {tickerPrice}, profit {tickerPrice - buyPrice}");

                    _currentBalance = _currentBalance * tickerPrice / buyPrice; //temporary

                    currentState = STATE.LOOKING_FOR_OPPORTUNITY;
                }
            }
        }

        private bool NeedToSell(string currency1, string currency2, decimal tickerPrice)
        {
            if (tickerPrice > sellPrice)
                return true;
            else if (_timeProvider.Now() > failTime)
            {
                _logger.Debug($"{_timeProvider.Now()} time elapsed");
                return true;
            }
            else if (tickerPrice < stopLossPrice)
            {
                _logger.Debug($"{_timeProvider.Now()} stop loss hit");
                return true;
            }
            else
                return false;
            
        }
        
        private Tuple<string, string> SelectMostPromisingPair(Dictionary<Tuple<string, string>, List<decimal>> pairsRsiResults)
        {
            decimal minRsi = 100;

            //take the ones which have at least one which have min rsi value below buy treshold and 
            pairsRsiResults = pairsRsiResults.Where(e => e.Value != null && e.Value.Count >= 2 &&
            e.Value.Min() <= _buyTreshold &&
            e.Value[e.Value.Count - 4] == e.Value.Min() && //take ones for which second last value is the lowest value
            e.Value[e.Value.Count - 4] < e.Value[e.Value.Count - 3] &&
            e.Value[e.Value.Count - 3] < e.Value[e.Value.Count - 2] &&
            e.Value[e.Value.Count - 2] < e.Value[e.Value.Count - 1]
            ) //and ones for which rsi is already raising
                .ToDictionary(k => k.Key, v => v.Value);

            if (pairsRsiResults.Any())
            {
                _logger.Debug($"{_timeProvider.Now().ToString()} Following pairs found:");

                foreach (var pair in pairsRsiResults)
                {
                    string rsiMsg = string.Empty;

                    foreach (var rs in pair.Value)
                        rsiMsg += $"{rs.ToString("#")} ";

                    _logger.Debug($"{_timeProvider.Now().ToString()} Pair: {pair.Key.Item1} {pair.Key.Item2} rsi {rsiMsg}");
                }

                foreach (var pair in pairsRsiResults)
                {
                    if (pair.Value.Min() < minRsi)
                    {
                        bestPair = pair.Key;
                        minRsi = pair.Value.Min();
                    }
                }
            }
            else if (pairsRsiResults.Count == 0)
            {
                bestPair = null;
                //_logger.Debug($"No opportunity in the market ritgh now");
                return bestPair;
            }

            _logger.Debug($"Pair selected: {bestPair.Item1} {bestPair.Item2}");

            return bestPair;
        }

        private async void SetBuyOrder(string currency1, string currency2, decimal price, decimal amount)
        {

        }

        //sell max
        private async void SetSellOrder(string currency1, string currency2, decimal price)
        {

        }

        private async Task<decimal> CheckAccountBalance()
        {
            return _currentBalance;
        }
        
        private async Task<bool> IsSellOrderFilled(string currency1, string currency2)
        {
            Ticker t = _exchange.GetTicker(bestPair.Item1, bestPair.Item2).GetAwaiter().GetResult();
           /* if (t.bid == int.MinValue)
                ++trick;*/
            //todo: skasować warunek z int.MinValue -> tylko potrzebne dla danych historycznych
            return sellPrice <= t.bid || t.bid == int.MinValue;
        }

        private async Task<bool> IsBuyOrderFilled(string currency1, string currency2)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">Needed to be able to actually test it on historical data</param>
        /// <param name="end">Needed to be able to actually test it on historical data</param>
        /// <returns></returns>
        private async Task<Tuple<string,string,decimal>> FindLowestRsiPair(DateTime startDate, DateTime end)
        {
            decimal lastLowestRSI = 100; //rsi cannot be higher than 100
            Tuple<string, string> bestPair = new Tuple<string, string>("", "");
            
            var tmp = new List<Task<Tuple<Tuple<string, string>, string>>>();

            _currenciesToWorkOn.ForEach(pair => tmp.Add(_exchange.GetHistoricalData(pair, startDate, end, _candlePeriod)));

            var allHistoricalData = await Task.WhenAll(tmp);
           
            foreach (var singleCrypto in allHistoricalData)
            {
                var pair = singleCrypto.Item1;
                var candlesJson = singleCrypto.Item2;

                List<Candle> candles = JsonConvert.DeserializeObject<List<Candle>>(candlesJson);
                List<decimal> prices = candles.Select(p => p.Close).ToList();

                Exchange.MarketUtils.RSI rsiCalc = new Exchange.MarketUtils.RSI();

                decimal rsi = rsiCalc.CalculateRSI(prices, _period);

                if (rsi < lastLowestRSI)
                {
                    lastLowestRSI = rsi;
                    bestPair = pair;
                }
                _logger.Debug($"Pair: {pair.Item1} {pair.Item2} rsi {rsi}");
            }

            if(bestPair != null)
                _logger.Debug($"bestPair: {bestPair.Item1} {bestPair.Item2} rsi {lastLowestRSI}");
            //buy @ current price

            //set sell order @ higher price
            return new Tuple<string, string, decimal>(bestPair.Item1, bestPair.Item2, lastLowestRSI);
        }


        /// <summary>
        /// looking for a pair for which rsi reached low value and is recovering
        /// </summary>
        /// <param name="end"></param>
        /// <param name="additionalCandles"></param>
        /// <returns></returns>
        private async Task<Dictionary<Tuple<string, string>, List<decimal>>> CalculateMultiplRsiPoints(DateTime startDate, DateTime end)
        {
            Dictionary<Tuple<string, string>, List<decimal>> result = new Dictionary<Tuple<string, string>, List<decimal>>(); 

            //if (candlesToAnalyze < _period || candlesToAnalyze < risingRsiCandles)
            //    throw new Exception("Need more data to calculate rsi for a time period");

            var tmp = new List<Task<Tuple<Tuple<string, string>, string>>>();

            _currenciesToWorkOn.ForEach(pair => tmp.Add(_exchange.GetHistoricalData(pair, startDate, end, _candlePeriod)));

            var allHistoricalData = await Task.WhenAll(tmp);

            //analyze single currency pair
            foreach (var singleCrypto in allHistoricalData)
            {
                var pair = singleCrypto.Item1;
                var candlesJson = singleCrypto.Item2;

                List<Candle> candles = JsonConvert.DeserializeObject<List<Candle>>(candlesJson);
                List<decimal> prices = candles.Select(p => p.Close).ToList();

                Exchange.MarketUtils.RSI rsiCalc = new Exchange.MarketUtils.RSI();

                //find currency for which rsi was dropping during last x periods/reached point below treschold and is recovering for y candles

                //calculate rsi for range of the points
                List<decimal> rsiSet = rsiCalc.CalcRsiForTimePeriod(prices, _period);

                result.Add(singleCrypto.Item1, rsiSet);

                //build the list of the results
                //select one which have rsi < treshold and biggest rsi increase between two last points

                //string rsiMsg = string.Empty;
                //
                //foreach (var rs in rsiSet)
                //    rsiMsg += $"{rs.ToString("#")} ";
                //
                //_logger.Debug($"Pair: {pair.Item1} {pair.Item2} rsi {rsiMsg}");
            }

            return result;
        }

        public decimal CurrentBalance()
        {
            return _currentBalance;
        }

    }
}