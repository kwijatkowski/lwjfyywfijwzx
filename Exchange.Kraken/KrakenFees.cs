using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Kraken
{
    class SingleCurrencyFees
    {
        public decimal[,] fees { get; set; }
        public decimal[,] fees_maker { get; set; }
        public string fee_volume_currency { get; set; }
    }

    class KrakenFees
    {
        public Dictionary<string, SingleCurrencyFees> TransactionFees { get; set; }
    }
}
