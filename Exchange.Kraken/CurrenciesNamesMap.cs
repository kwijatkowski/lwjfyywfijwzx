using System.Collections.Generic;
using Exchange.MarketUtils;

namespace Exchange.Kraken
{
    public static class CurrenciesNamesMap
    {
        public static Dictionary<string, string> MapCrypto
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {Currencies.Bitcoin, "XBT"},
                    {Currencies.BitcoinCash, "BCH"},
                    {Currencies.Dash, "DASH"},
                    {Currencies.Dogecoin, "XDG"},
                    {Currencies.Eos, "EOS"},
                    {Currencies.Ether, "ETH"},
                    {Currencies.EtherClassic, "ETC"},
                    {Currencies.Gnosis, "GNO"},
                    {Currencies.Iconomi, "ICN"},
                    {Currencies.Litecoin, "LTC"},
                    {Currencies.Monero, "XMR"},
                    {Currencies.Namecoin, "NMC"},
                    {Currencies.Ripple, "XRP"},
                    {Currencies.Stellar, "XLM"},
                    {Currencies.Tether, "USDT"},
                    {Currencies.Zcash, "ZEC"},
                };
            }
        }

        public static Dictionary<string, string> MapFiat
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {Currencies.CAD, "CAD"},
                    {Currencies.EUR, "EUR" },
                    {Currencies.JPY, "JPY"},
                    {Currencies.USD, "USD" },
                };
            }
        }


        //public static Dictionary<string, string> MapCrypto { get { return _mapCrypto; } }
        //public static Dictionary<string, string> MapFiat { get { return _mapFiat; } }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Kraken currency symbol</returns>
        public static string MapName(string name)
        {
            string symbol = string.Empty;

            if (MapCrypto.TryGetValue(name, out symbol))
                return symbol;
            else if (MapFiat.TryGetValue(name, out symbol))
                return symbol;
            else
                throw new System.Exception($"Currency symbol not defined for name {name}");
        }

        public static Dictionary<string, string> All()
        {
            var all = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> pair in MapCrypto)
                all.Add(pair.Key, pair.Value);

            foreach (KeyValuePair<string, string> pair in MapFiat)
                all.Add(pair.Key, pair.Value);

            return all;
        }
    }
}