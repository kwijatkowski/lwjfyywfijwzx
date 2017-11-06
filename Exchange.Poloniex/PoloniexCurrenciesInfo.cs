using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Poloniex
{
    public class PoloniexCurrenciesInfo
    {
        public int id;
        public string name;
        public decimal txFee;
        public decimal minConf;
        public string depositAddress;
        public int disabled;
        public int delisted;
        public int frozen;
    }
}
