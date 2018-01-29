using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exchange.MarketUtils;
using Exchange.Poloniex;
using Exchange.Mock;
using log4net;
using Newtonsoft.Json;

namespace Strategy.DailyChanges
{
    public class Daily
    {
        private enum STATE { LOOKING_FOR_OPPORTUNITY, BUYING, IN_POSITION, SELLING }


        private Poloniex _exchange;
        private List<Tuple<string, string>> _currenciesToWorkOn;
        private decimal _buyTreshold;
        private int _periodsToTakeToCalcRsi;
        private int _shorterCandlePeriod;
        private int _longerCandlePeriod;
        private ILog _logger;
        private ITimeProvider _timeProvider;
        private decimal _currentBalance;
        private STATE currentState;

        private decimal buyPrice;
        private decimal sellPrice;
        private decimal _targetProfitPercentage;
        private Tuple<string, string> bestPair;

        private decimal stopLossPrice;
        private DateTime failTime;


        public Daily(Poloniex exchange, List<Tuple<string, string>> currenciesToWorkOn, decimal rsiBuyTreshold, int periodsToTakeToCalcRsi, int shorterCandlePeriod, int longerCandlePeriod, decimal targetProfitPercentage, decimal startBalance, ILog logger, ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _exchange = exchange;
            _currenciesToWorkOn = currenciesToWorkOn;
            _buyTreshold = rsiBuyTreshold;
            _periodsToTakeToCalcRsi = periodsToTakeToCalcRsi;
            _longerCandlePeriod = longerCandlePeriod;
            _shorterCandlePeriod = shorterCandlePeriod;
            _logger = logger;
            _targetProfitPercentage = targetProfitPercentage;
            _currentBalance = startBalance;
            currentState = STATE.LOOKING_FOR_OPPORTUNITY;
        }


        //runs in the loop
        public async Task Run()
        {
            if (currentState == STATE.LOOKING_FOR_OPPORTUNITY)
            {
                Dictionary<Tuple<string, string>, decimal> rsiForPairs = new Dictionary<Tuple<string, string>, decimal>();

                //to do: download test data for longer period candles

                //get data for rsi calculation on daily chart
                //calculate daily rsi for all pairs which we want to process
                foreach (var pair in _currenciesToWorkOn)
                {
                    int additionalRsiPointsLong = 0; // one is calculated by default, so we will get additionalRsiPoints + 1
                    DateTime longerTermDataStart = _timeProvider.Now() - new TimeSpan(0, 0, (_periodsToTakeToCalcRsi + 1 + additionalRsiPointsLong) * _longerCandlePeriod);
                    DateTime longerTermDataEnd = _timeProvider.Now();
                    var results = await _exchange.GetHistoricalData(pair, longerTermDataStart, longerTermDataEnd, _longerCandlePeriod);

                    //todo: update it - performance hit
                    List<Candle> candles = JsonConvert.DeserializeObject<List<Candle>>(results.Item2);
                    List<decimal> prices = candles.Select(p => p.Close).ToList();


                    RSI rsiCalc = new RSI();
                    rsiForPairs.Add(new Tuple<string, string>(pair.Item1, pair.Item2), rsiCalc.CalculateRSI(prices, _periodsToTakeToCalcRsi));
                }

                List<Tuple<string, string>> pairsForFurtherProcessing = SelectPairsBasedOnRsiData(rsiForPairs);
                Dictionary<Tuple<string, string>, List<decimal>> RsiSetsForSelectedCurrencies = new Dictionary<Tuple<string, string>, List<decimal>>();

                //get 30 min data for selected set of the currencies
                //same as above

                int additionalRsiPoints = 4; // Need it if we want to biuld rsi set. One is calculated by default, so we will get additionalRsiPoints + 1
                DateTime shorterTermDataStart = _timeProvider.Now() - new TimeSpan(0, 0, (_periodsToTakeToCalcRsi + 1 + additionalRsiPoints) * _shorterCandlePeriod);
                DateTime shorterTermDataEnd = _timeProvider.Now();

                RsiSetsForSelectedCurrencies = await CalculateMultiplRsiPoints(shorterTermDataStart, shorterTermDataEnd, pairsForFurtherProcessing, _shorterCandlePeriod, _periodsToTakeToCalcRsi);

                //check trend
                //get 24h low and 24h high
                //check lower/upper half
                RsiSetsForSelectedCurrencies = CheckTrendAndFilterPairs(RsiSetsForSelectedCurrencies);

                //buy and set sell order
                bestPair = SelectBestPair(RsiSetsForSelectedCurrencies);

                if (bestPair != null)
                {
                    //buy
                    //set sell order
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
                    _logger.Debug($"{_timeProvider.Now()} Sell order filled {bestPair.Item1} {bestPair.Item2} price {tickerPrice}");
                    _logger.Debug($"Sold {bestPair.Item1} {bestPair.Item2} @ {tickerPrice}, profit {tickerPrice - buyPrice}");

                    _currentBalance = _currentBalance * tickerPrice / buyPrice; //temporary

                    currentState = STATE.LOOKING_FOR_OPPORTUNITY;
                }

            }
        }

            List<Tuple<string,string>> SelectPairsBasedOnRsiData(Dictionary<Tuple<string,string>,decimal> allPairsRsiDataLongerTerm)
        {
            return new List<Tuple<string, string>>();
        }

        /// <summary>
        /// looking for a pair for which rsi reached low value and is recovering
        /// </summary>
        /// <param name="end"></param>
        /// <param name="additionalCandles"></param>
        /// <returns></returns>
        private async Task<Dictionary<Tuple<string, string>, List<decimal>>> CalculateMultiplRsiPoints(DateTime startDate, DateTime end, List<Tuple<string,string>> currenciesToWorkOn, int shorterCandlePeriod, int periodsToTakeToCalcRsi)
        {
            Dictionary<Tuple<string, string>, List<decimal>> result = new Dictionary<Tuple<string, string>, List<decimal>>();

            var tmp = new List<Task<Tuple<Tuple<string, string>, string>>>();

            currenciesToWorkOn.ForEach(pair => tmp.Add(_exchange.GetHistoricalData(pair, startDate, end, shorterCandlePeriod)));

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
                List<decimal> rsiSet = rsiCalc.CalcRsiForTimePeriod(prices, periodsToTakeToCalcRsi);

                result.Add(singleCrypto.Item1, rsiSet);

            }

            return result;
        }

        private Dictionary<Tuple<string, string>, List<decimal>> CheckTrendAndFilterPairs(Dictionary<Tuple<string, string>, List<decimal>> input)
        {
            //todo: check trend and return only pairs which are ok

            return input;
        }


        private Tuple<string,string> SelectBestPair(Dictionary<Tuple<string, string>, List<decimal>> input)
        {
            //todo: extend to make it working for more than one pair. Using one only for proof of concept

            bool DontHaveNiceOne = true;

            if (DontHaveNiceOne)
                return null;

            return input.First().Key;
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

    }
}
