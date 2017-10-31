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
        private static void Main(string[] args)
        {
            string privateApiAddress = "https://bitbay.net/API/Trading/tradingApi.php";
            string publicApiAddress = "https://bitbay.net/API/Public/";
            string toDisplay = "done";
            string currency1 = "BTC";
            string currency2 = "USD";
            string resultPath = @"C:\Projects\Priv\Bot\dataFormats\bitbayPublic\";
            string jsonExtension = ".json";

            bool writeFiles = false;
            bool connectToPublicApi = false;

            bool connectToPrivateApi = true;

            Task.Run(async () =>
            {
                if (connectToPublicApi)
                {
                    Exchange.BitBay.PublicApiConnector publicConnector = new PublicApiConnector(publicApiAddress);
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
                }

                if (connectToPrivateApi)
                {
                    //documentation 
                    // https://bitbay.net/en/api-private

                    string publicKey = "";//your api public key
                    string privateKey = "";//your api private key

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("method", "history");
                    parameters.Add("currency", "PLN");
                    parameters.Add("limit", "10");

                    Exchange.BitBay.PrivateApiConnector privateConnector = new Exchange.BitBay.PrivateApiConnector(privateApiAddress, publicKey,privateKey);
                    Console.WriteLine(FormatJson(await privateConnector.GetData(parameters)));
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