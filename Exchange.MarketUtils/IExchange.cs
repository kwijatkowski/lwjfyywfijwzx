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
        void GetOrderbook();
    }
}
