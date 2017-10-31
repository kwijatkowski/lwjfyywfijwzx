using Exchange.BitBay;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;

namespace startup
{
    internal class TestApp
    {
        public static class BitbayAPI
        {
            public static string publicApiAddress { get { return "https://bitbay.net/API/Trading/tradingApi.php"; } }
            public static string privateApiAddress { get { return "https://bitbay.net/API/Public/"; } }
        }

        public static class KrakenAPI
        {
            public static string publicApiAddress { get { return "https://api.kraken.com/0/public/"; } }
            //public static string privateApiAddress { get { return "https://api.kraken.com/0/public/"; } }
        }

        private static void Main(string[] args)
        {

            //string privateApiAddress = "https://bitbay.net/API/Trading/tradingApi.php";
            //string publicApiAddress = "https://bitbay.net/API/Public/";
            string toDisplay = "done";
            string currency1 = "BTC";
            string currency2 = "USD";
            string resultPath = @"C:\Projects\Priv\Bot\dataFormats\bitbayPublic\";
            string jsonExtension = ".json";

            bool writeFiles = false;
            bool connectToPublicApi = true;

            bool connectBitbay = false;
            bool connectKraken = true;

            bool connectToPrivateApi = false;

            Task.Run(async () =>
            {
                if (connectToPublicApi)
                {
                    if (connectBitbay)
                    {
                        #region Bitbay
                        Exchange.BitBay.PublicApiConnector publicConnector = new PublicApiConnector(BitbayAPI.publicApiAddress);
                        string all = FormatJson(await publicConnector.GetAll(currency1, currency2));
                        string market = FormatJson(await publicConnector.GetMarket(currency1, currency2));
                        string orderbook = FormatJson(await publicConnector.GetOrderbook(currency1, currency2));
                        string ticker = FormatJson(await publicConnector.GetTicker(currency1, currency2));
                        string trades = FormatJson(await publicConnector.GetTrades(currency1, currency2));

                        if (writeFiles)
                        {
                            File.WriteAllText(string.Concat(resultPath, "all", jsonExtension), all);
                            File.WriteAllText(string.Concat(resultPath, "market", jsonExtension), market);
                            File.WriteAllText(string.Concat(resultPath, "orderbook", jsonExtension), orderbook);
                            File.WriteAllText(string.Concat(resultPath, "ticker", jsonExtension), ticker);
                            File.WriteAllText(string.Concat(resultPath, "trades", jsonExtension), trades);
                        }

                        Console.Write(all);
                        #endregion
                    }
                    
                    if (connectKraken)
                    {
                        #region kraken

                        Exchange.Kraken.PublicApiConnector connector = new Exchange.Kraken.PublicApiConnector(KrakenAPI.publicApiAddress);
                        //Console.WriteLine(FormatJson(await connector.GetServerTime()));
                        //Console.WriteLine(FormatJson(await connector.GetAssets()));
                        //Console.WriteLine(FormatJson(await connector.GetAssetPairs()));                       
                        //Console.WriteLine(FormatJson(await connector.GetTicker("BCHEUR,BCHUSD")));
                        //Console.WriteLine(FormatJson(await connector.GetOHLCdata("BCHEUR")));
                        //Console.WriteLine(FormatJson(await connector.GetOrderBook("BCHEUR")));
                        //Console.WriteLine(FormatJson(await connector.GetRecentTrades("BCHEUR")));
                        Console.WriteLine(FormatJson(await connector.GetRecentSpreadData("BCHEUR")));
                        #endregion
                    }
                }

                if (connectToPrivateApi)
                {
                    #region bitbay
                    //documentation 
                    // https://bitbay.net/en/api-private

                    string publicKey = "";//your api public key
                    string privateKey = "";//your api private key

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("method", "history");
                    parameters.Add("currency", "PLN");
                    parameters.Add("limit", "10");

                    Exchange.BitBay.PrivateApiConnector privateConnector = new Exchange.BitBay.PrivateApiConnector(BitbayAPI.privateApiAddress, publicKey,privateKey);
                    Console.WriteLine(FormatJson(await privateConnector.GetData(parameters)));
                    #endregion
                }

            }).GetAwaiter().GetResult();

            Console.Write(toDisplay);
            Console.ReadLine();
        }

        public static string FormatJson(string toFormat)
        {
            return JValue.Parse(toFormat).ToString(Formatting.Indented);
        }
    }
}