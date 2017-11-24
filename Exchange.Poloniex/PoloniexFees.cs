using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Poloniex
{
    internal class PoloniexFees
    {        
        public TransferFee[] transfer { get; set; }
        public TransactionFee[] transaction { get; set; }
    }

    class TransferFee
    {
        public string currency { get; set; }
        public decimal incoming { get; set; }
        public decimal outgoing { get; set; }
        public string feeType { get; set; }
    }

    class TransactionFee
    {
        public decimal treshold { get; set; }
        public string tresholdCurrency { get; set; }
        public decimal maker { get; set; }
        public decimal taker { get; set; }
        public string feeType { get; set; }
    }

}
