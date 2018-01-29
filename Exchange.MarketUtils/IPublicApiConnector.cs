using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public interface IPublicApiConnector
    {
        Task<string> GetTicker(string currency1 = null, string currency2 = null);
        Task<string> GetCurrencies();

        Task<string> GetOrderBook(string pair, int? count = null);

        Task<string> GetChartData(string pair, long start, long end, int period);

        Task<string> Get24hVolume();

    }
}
