using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public interface IExchange
    {
        /// <summary>
        /// Gets name of exchange
        /// </summary>
        /// <returns>Name of exchange</returns>
        string GetName();
        Task<Ticker> GetTicker(string currency1, string currency2);
        Task<OrderBook> GetOrderbook(string currency1, string currency2, int? countLimit = null);

        //List<string> GetTradablePairs();

        /// <summary>
        /// Returning pair in correct order if it is acceptable by exchange. If not null
        /// </summary>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <returns></returns>
        Tuple<string,string> MakeValidPair(string currency1, string currency2, out bool inverted);

        decimal CalculateTransacionFee(string startCurrency, string targetCurrency);

        /// <summary>
        /// Calculating withdrawal fee from exchange
        /// </summary>
        /// <param name="currency">Currency to be withdrawn</param>
        /// <param name="volume">Volume if applicable</param>
        /// <returns></returns>
        decimal CalculateTransferFee(string currency, decimal volume = 0);
    }
}
