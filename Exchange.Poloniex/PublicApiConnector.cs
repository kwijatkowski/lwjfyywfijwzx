using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Exchange.Poloniex
{
    public class PublicApiConnector
    {
        //documentation
        //https://m.poloniex.com/support/api/

        private readonly string _baseAddress;

        public PublicApiConnector(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        //Appropriate labels for these data are, in order: currencyPair, last, lowestAsk, highestBid, percentChange, baseVolume, quoteVolume, isFrozen, 24hrHigh, 24hrLow
        public async Task<string> GetTicker()
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

        //public async Task<string> GetServerTime()
        //{
        //    string reativaAddress = "Time";
        //    return await GetDataFromAddress<string>(reativaAddress);
        //}

        //public async Task<string> GetFees()
        //{
        //    string method = "AssetPairs";
        //    Dictionary<string, string> parameters =
        //        new Dictionary<string, string>() {
        //        { "info","fees" }
        //        };

        //    return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, method, parameters));
        //}

        //public async Task<string> GetAssets(Dictionary<string, string> parameters = null)
        //{
        //    string method = "Assets";
        //    return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, method, parameters));
        //}

        //public async Task<string> GetAssetPairs(Dictionary<string, string> parameters = null)
        //{
        //    string method = "AssetPairs";
        //    return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, method, parameters));
        //}

        ///// <summary>
        ///// Obtains array of pair names and their ticker info
        ///// </summary>
        ///// <param name="pair">pair = comma delimited list of asset pairs to get info on</param>
        ///// <returns></returns>
        //public async Task<string> GetTicker(string pair)
        //{
        //    //BCHEUR pair
        //    string method = "Ticker";

        //    Dictionary<string, string> parameters =
        //        new Dictionary<string, string>() {
        //        { "pair", pair }
        //        };

        //    return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, method, parameters));
        //}

        //public async Task<string> GetOHLCdata(Dictionary<string, string> parameters)
        //{
        //    //https://api.kraken.com/0/public/OHLC?pair=BCHUSD&interval=5
        //    //BCHEUR pair

        //    string method = "OHLC";
        //    return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, method, parameters));
        //}

        ////todo: pass count param
        //public async Task<string> GetOrderBook(Dictionary<string, string> parameters)
        //{
        //    //https://api.kraken.com/0/public/OHLC?pair=BCHUSD&interval=5
        //    //BCHEUR pair
        //    string method = "Depth";
        //    return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, method, parameters));
        //}

        ////todo: pass since param to get less data
        //public async Task<string> GetRecentTrades(Dictionary<string, string> parameters)
        //{
        //    //https://api.kraken.com/0/public/OHLC?pair=BCHUSD&interval=5
        //    //BCHEUR pair
        //    string method = "Trades";
        //    return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, method, parameters));
        //}

        ////todo: pass since param
        //public async Task<string> GetRecentSpreadData(Dictionary<string, string> parameters)
        //{
        //    //https://api.kraken.com/0/public/OHLC?pair=BCHUSD&interval=5
        //    //BCHEUR pair
        //    string method = "Spread";
        //    return await GetDataFromAddress<string>(BuildRequestUrl(_baseAddress, method, parameters));
        //}

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
