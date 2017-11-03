using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public static class OperationTypes
    {
        public  enum TRANSFER_DIR { incomming, outgoing };
        public  enum FEE_TYPE { absolute, fraction };
        public  enum OPERATION_TYPE { maker, taker };
    }
}
