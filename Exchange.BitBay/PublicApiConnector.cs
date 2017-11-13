﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Exchange.BitBay
{
    public class PublicApiConnector
    {
        private string _baseAddress;

        //keywords
        private string trades = "trades";

        private string orderbook = "orderbook";
        private string market = "market";
        private string ticker = "ticker";
        private string all = "all";

        private string jsonExtension = ".json";

        /// <summary>
        ///
        /// </summary>
        /// <param name="baseAddress">like https://bitbay.net/API/Public/ </param>
        public PublicApiConnector(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        public async Task<List<Tuple<string,string>>> GetTradablePairs()
        {
            string address = "https://api.bitbay.net/rest/trading/ticker";
            string response = await GetDataFromAddress<string>(null, address);

            List<Tuple<string, string>> tradablePairs = new List<Tuple<string, string>>();

            JObject j = JObject.Parse(response);
            var resultJson = j.SelectToken("items").ToString();
            Dictionary<string, object> pairsInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultJson);
            var pairs = pairsInfo.Keys.ToList();

            foreach (var pair in pairs)
            {
                string first = pair.Split('-')[0];
                string second = pair.Split('-')[1];
                tradablePairs.Add(new Tuple<string, string>(first, second));
            }

            return tradablePairs;
        }

        public async Task<string> GetTrades(string currency1, string currency2)
        {
            string relativeAddress = string.Concat(currency1, currency2, "/", trades, jsonExtension);
            return await GetDataFromAddress<string>(relativeAddress);
        }

        public async Task<string> GetOrderbook(string currency1, string currency2)
        {
            string relativeAddress = string.Concat(currency1, currency2, "/", orderbook, jsonExtension);
            return await GetDataFromAddress<string>(relativeAddress);
        }

        public async Task<string> GetMarket(string currency1, string currency2)
        {
            string relativeAddress = string.Concat(currency1, currency2, "/", market, jsonExtension);
            return await GetDataFromAddress<string>(relativeAddress);
        }

        public async Task<string> GetTicker(string currency1, string currency2)
        {
            string relativeAddress = string.Concat(currency1, currency2, "/", ticker, jsonExtension);
            return await GetDataFromAddress<string>(relativeAddress);
        }

        public async Task<string> GetAll(string currency1, string currency2)
        {
            string relativeAddress = string.Concat(currency1, currency2, "/", all, jsonExtension);
            return await GetDataFromAddress<string>(relativeAddress);
        }

        private async Task<T> GetDataFromAddress<T>(string relativeAddress, string absolute = null)
        {
            //todo: think if need to automaticaly close connection
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseAddress);
                HttpResponseMessage response;

                if (relativeAddress != null)
                    response = await client.GetAsync(string.Concat(_baseAddress, relativeAddress));
                else
                    response = await client.GetAsync(absolute);

                Type desiredType = typeof(T);

                if (desiredType == typeof(string))
                {
                    return (T)Convert.ChangeType(await response.Content.ReadAsStringAsync(), desiredType);
                }

                throw new InvalidOperationException("unknown requested data type");
            }
            //  string data = await response.Content.ReadAsStringAsync();
        }

        //https://bitbay.net/API/Public/[Waluta 1][Waluta 2]/[Kategoria].json
        //Currency 1 - necessary, shortcut of cryptocurrency(BTC or LTC)
        //Currency 2 - optional, if not specified default currency is USD
        //Category - type of request, available types:
        //trades - last transactions
        //orderbook - orders from stock market
        //market - orders from stock market and last transactions
        //ticker - basic statistics
        //all - all above informations combined in one object
    }
}