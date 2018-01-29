using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Exchange.MarketUtils;

namespace Exchange.Poloniex
{
    public class PublicApiConnector : IPublicApiConnector
    {
        //documentation
        //https://m.poloniex.com/support/api/

        private readonly string _baseAddress;

        public PublicApiConnector(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        //Appropriate labels for these data are, in order: currencyPair, last, lowestAsk, highestBid, percentChange, baseVolume, quoteVolume, isFrozen, 24hrHigh, 24hrLow
        public async Task<string> GetTicker(string currency1 = null, string currency2 = null)
        {
            string relative = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "command", "returnTicker"}
            };

            return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, relative, parameters)); 
        }

        public async Task<string> GetCurrencies()
        {
            string relative = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "command", "returnCurrencies"}
            };

            return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, relative, parameters));
        }

        //https://poloniex.com/public?command=return24hVolume
        public async Task<string> Get24hVolume()
        {
            string relative = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "command", "return24hVolume"}
            };

            return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, relative, parameters));

        }

        public async Task<string> GetOrderBook(string pair, int? count = null)
        {
            string relative = string.Empty;

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "command", "returnOrderBook"},
                { "currencyPair", pair}
            };

            if (count != null)
                parameters.Add("depth", count.ToString());

            return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, relative, parameters));
        }

        public async Task<string> GetChartData(string pair, long start, long end, int period)
        {
            string relative = string.Empty;

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "command", "returnChartData"},
                { "currencyPair", pair},
                { "end", end.ToString()},
                { "period", period.ToString()},
                { "start", start.ToString()}
            };

            string returnedData = await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, relative, parameters));
            return returnedData;
        }


        //  HELP

        private string BuildRequestUrl(string baseAddress, string method, Dictionary<string, string> parameters = null)
        {
            if (parameters != null)
            {
                NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

                foreach (var pair in parameters)
                    queryString[pair.Key] = pair.Value;

                return string.Concat(baseAddress, method, "?", queryString.ToString());
            }
            else
                return string.Concat(baseAddress, method);
        }

        private async Task<T> GetDataFromAddress<T>(string address)
        {
            //todo: think if need to automaticaly close connection
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseAddress);

                HttpResponseMessage response = await client.GetAsync(address);

                Type desiredType = typeof(T);

                if (desiredType == typeof(string))
                {
                    return (T)Convert.ChangeType(await response.Content.ReadAsStringAsync(), desiredType);
                }

                throw new InvalidOperationException("unknown requested data type");
            }
            //  string data = await response.Content.ReadAsStringAsync();
        }
    }
}
