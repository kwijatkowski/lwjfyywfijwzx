namespace Exchange.Poloniex
{
    public class PoloniexTicker
    {
        public int id;
        public decimal last;
        public decimal lowestAsk;
        public decimal highestBid;
        public decimal percentChange;
        public decimal baseVolume;
        public decimal quoteVolume;
        public int isFrozen;
        public decimal high24hr;
        public decimal low24hr;
    }
}