using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.MarketUtils
{
    public interface IOperationFeesCalculator
    {
        decimal CalculateTransferCost(string currency, OperationTypes.TRANSFER_DIR direction, decimal transferAmount = -1);
        decimal CalculateTransactionFee(string currencyPair, OperationTypes.OPERATION_TYPE operationType, decimal volume);

    }
}
