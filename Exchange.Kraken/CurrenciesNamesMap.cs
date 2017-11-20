using Exchange.MarketUtils;
using System.Collections.Generic;
using System;
using System.Linq;

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
                    {Currencies.Bitcoin, KrakenCurrencies.XBT},
                    {Currencies.BitcoinCash, KrakenCurrencies.BCH},
                    {Currencies.Dash, KrakenCurrencies.DASH},
                    {Currencies.Dogecoin, KrakenCurrencies.XDG},
                    {Currencies.Eos, KrakenCurrencies.EOS},
                    {Currencies.Ethereum, KrakenCurrencies.ETH},
                    {Currencies.EthereumClassic, KrakenCurrencies.ETC},
                    {Currencies.Gnosis, KrakenCurrencies.GNO},
                    {Currencies.Iconomi, KrakenCurrencies.ICN},
                    {Currencies.Litecoin, KrakenCurrencies.LTC},
                    {Currencies.Melon, KrakenCurrencies.MLN},
                    {Currencies.Monero, KrakenCurrencies.XMR},
                    {Currencies.Namecoin, KrakenCurrencies.NMC},
                    {Currencies.Ripple, KrakenCurrencies.XRP},
                    {Currencies.Stellar, KrakenCurrencies.XLM},
                    {Currencies.Tether, KrakenCurrencies.USDT},
                    {Currencies.Zcash, KrakenCurrencies.ZEC},
                };
            }
        }

        public static Dictionary<string, string> MapFiat
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    {Currencies.CAD, KrakenCurrencies.CAD },
                    {Currencies.EUR, KrakenCurrencies.EUR },
                    {Currencies.JPY, KrakenCurrencies.JPY },
                    {Currencies.USD, KrakenCurrencies.USD },
                    {Currencies.GBP, KrakenCurrencies.GBP }
                };
            }
        }

        private static List<string> _assetsWithoutPrefix = new List<string>
        {
            MapCrypto[Currencies.BitcoinCash], // KrakenCurrencies.BCH,
            MapCrypto[Currencies.Dash], // KrakenCurrencies.DASH,           
            MapCrypto[Currencies.Eos], // KrakenCurrencies.EOS,
            MapCrypto[Currencies.Gnosis] // "GNO"
        };

        private static bool isFiat(string mappedNameNoPrefix)
        {
            return MapFiat.Any( i => i.Value == mappedNameNoPrefix);
        }

        private static bool isCrypto(string mappedNameNoPrefix)
        {
            return MapCrypto.Any(i => i.Value == mappedNameNoPrefix);
        }

        internal static string GetPrefix(string mappedNoPrefix)
        {
            string fiatPrefix = "Z";
            string cryptoPrefix = "X";

            if (_assetsWithoutPrefix.Contains(mappedNoPrefix))
                return string.Empty;
            else
            {
                if (isCrypto(mappedNoPrefix))
                    return cryptoPrefix;
                else if (isFiat(mappedNoPrefix))
                    return fiatPrefix;
            }

            throw new Exception($"invalid currency name {mappedNoPrefix}");
        }

        //public static Dictionary<string, string> MapCrypto { get { return _mapCrypto; } }
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
            {
                //return symbol;
            }
            else if (MapFiat.TryGetValue(name, out symbol))
            {
                //return symbol;
            }
            else
                throw new System.Exception($"Currency symbol not defined for name {name}");

            return string.Concat(GetPrefix(symbol), symbol);
        }

        public static Dictionary<string, string> All()
        {
            var all = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> pair in MapCrypto)
                all.Add(pair.Key, MapNameToSymbol(pair.Key));

            foreach (KeyValuePair<string, string> pair in MapFiat)
                all.Add(pair.Key, MapNameToSymbol(pair.Key));

            return all;
        }
    }
}