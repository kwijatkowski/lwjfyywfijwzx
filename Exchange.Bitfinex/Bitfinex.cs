using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Bitfinex
{
    public class Bitfinex
    {
        private string _publicApiURL;
        private string _privateApiURL;

       // private PublicApiConnector _publicApiConnector;
        //private PrivateApiConnector publicApiConnector;

        public Bitfinex(string publicApiAddress, string privateApiAddress)
        {
            _publicApiURL = publicApiAddress;
            _privateApiURL = privateApiAddress;

          //  _publicApiConnector = new PublicApiConnector(_publicApiURL);
        }

        public string GetName()
        {
            return "Bitfinex";
        }

    }
}
