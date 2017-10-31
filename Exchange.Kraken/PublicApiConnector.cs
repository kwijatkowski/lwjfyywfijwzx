using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Exchange.Kraken
{
    public class PublicApiConnector
    {
        //documentation
        //https://www.kraken.com/help/api

        private readonly string _baseAddress;

        public PublicApiConnector(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        public async Task<string> GetServerTime()
        {
            string reativaAddress = "Time";
            return await GetDataFromAddress<string>(reativaAddress);
        }

        //todo: pass parameters to obtain less data
        public async Task<string> GetAssets()
        {
            string reativaAddress = "Assets";
            return await GetDataFromAddress<string>(reativaAddress);
        }

        //todo: pass parameters to obtain less data
        public async Task<string> GetAssetPairs()
        {
            string reativaAddress = "AssetPairs";
            return await GetDataFromAddress<string>(reativaAddress);
        }

        public async Task<string> GetTicker(string pairs)
        {
            //BCHEUR pair
            string reativaAddress = $"Ticker?pair={pairs}";
            return await GetDataFromAddress<string>(reativaAddress);
        }

        //todo: pass interval and since param
        public async Task<string> GetOHLCdata(string pair)
        {
            //https://api.kraken.com/0/public/OHLC?pair=BCHUSD&interval=5
            //BCHEUR pair
            string reativaAddress = $"OHLC?pair={pair}";
            return await GetDataFromAddress<string>(reativaAddress);
        }

        //todo: pass count param
        public async Task<string> GetOrderBook(string pair)
        {
            //https://api.kraken.com/0/public/OHLC?pair=BCHUSD&interval=5
            //BCHEUR pair
            string reativaAddress = $"Depth?pair={pair}";
            return await GetDataFromAddress<string>(reativaAddress);
        }

        //todo: pass since param to get less data
        public async Task<string> GetRecentTrades(string pair)
        {
            //https://api.kraken.com/0/public/OHLC?pair=BCHUSD&interval=5
            //BCHEUR pair
            string reativaAddress = $"Trades?pair={pair}";
            return await GetDataFromAddress<string>(reativaAddress);
        }

        //todo: pass since param
        public async Task<string> GetRecentSpreadData(string pair)
        {
            //https://api.kraken.com/0/public/OHLC?pair=BCHUSD&interval=5
            //BCHEUR pair
            string reativaAddress = $"Spread?pair={pair}";
            return await GetDataFromAddress<string>(reativaAddress);
        }

        private async Task<T> GetDataFromAddress<T>(string relativeAddress)
        {
            //todo: think if need to automaticaly close connection
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseAddress);

                HttpResponseMessage response = await client.GetAsync(string.Concat(_baseAddress, relativeAddress));

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