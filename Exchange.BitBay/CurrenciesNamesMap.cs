using System.Collections.Generic;
using Exchange.MarketUtils;

namespace Exchange.BitBay
{
    public static class CurrenciesNamesMap
    {
        public static Dictionary<string, string> MapCrypto
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {Currencies.Bitcoin, "BTC"},
                    {Currencies.Litecoin, "LTC"},
                    {Currencies.Lisk, "LSK"},
                    {Currencies.Ethereum, "ETH"},
                    {Currencies.Dash, "DASH"},
                    {Currencies.Game, "GAME"},
                    {Currencies.BitcoinCash, "BCC"},
                    {Currencies.BitcoinGold, "BTG"},
                };
            }
        }

        public static Dictionary<string, string> MapFiat
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {Currencies.EUR, "EUR" },
                    {Currencies.USD, "USD" },
                    {Currencies.PLN, "PLN" }
                };
            }
        }

        //public static Dictionary<string, string> MapCrypto {get {return _mapCrypto;} }
        //public static Dictionary<string, string> MapFiat { get { return _mapFiat; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Kraken currency symbol</returns>
        public static string MapNameToSymbol(string name)
        {
            string symbol = string.Empty;

            if (MapCrypto.TryGetValue(name, out symbol))
                return symbol;
            else if (MapFiat.TryGetValue(name, out symbol))
                return symbol;
            else
                throw new System.Exception($"Currency symbol not defined for name {name}");

        }        

        public static Dictionary<string,string> All()
        {
            var all = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> pair in MapCrypto)
                all.Add(pair.Key,pair.Value);

            foreach (KeyValuePair<string, string> pair in MapFiat)
                all.Add(pair.Key, pair.Value);

            return all;
        }
    }
}