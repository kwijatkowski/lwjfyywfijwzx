using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Exchange.MarketUtils
{
    // example exchange config file:

    //{
    //  "Name": "Kraken",
    //  "publicApiAddress": "publicApi",
    //  "privateApiAddress": "privateApi",
    //  "privateApiPublicKey": "publicKey",
    //  "privateApiPrivateKey": "privateKey"
    //}

public class ExchangeConfig
    {
        public string Name { get; set; }
        public string publicApiAddress { get; set; }
        public string privateApiAddress { get; set; }
        public string privateApiPublicKey { get; set; }
        public string privateApiPrivateKey { get; set; }

        public void Load(string configFilePath)
        {
            if (!File.Exists(configFilePath))
                throw new Exception($"Configuration file not found at path {configFilePath}");

            var config = JsonConvert.DeserializeObject<ExchangeConfig>(File.ReadAllText(configFilePath));
            Name = config.Name;
            publicApiAddress = config.publicApiAddress;
            privateApiAddress = config.privateApiAddress;
            privateApiPublicKey = config.privateApiPublicKey;
            privateApiPrivateKey = config.privateApiPrivateKey;
        }
    }
}
