using Exchange.MarketUtils;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Exchange.Poloniex
{
    public class OperationCostCalculator : IOperationFeesCalculator
    {
        private PoloniexFees _fees;
        private decimal _accountMonthlyVolume;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feesJson"></param>
        /// <param name="accountMonthlyVolume">Currency here is BTC</param>
        public OperationCostCalculator(string feesJson, decimal accountMonthlyVolume)
        {
            _fees = JsonConvert.DeserializeObject<PoloniexFees>(feesJson);
            _accountMonthlyVolume = accountMonthlyVolume;
        }

        /// <summary>
        /// Calculating cost of transfer operation
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="inOut"></param>
        /// <returns>Transfer cost in the same currency as provided</returns>
        public decimal CalculateTransferCost(string currency, OperationTypes.TRANSFER_DIR direction, decimal transferAmount = -1)
        {
            //poloniex fees are network fees
            return 0; //todo: figure out how to get network fees
        }

        /// <summary>
        /// Returning % fee based on fee schedule. 1 == 100%
        /// </summary>
        /// <param name="operationType"></param>
        /// <param name="volume">In this case it is monthly volume EUR</param>
        /// <returns>Fee in currency. To calculate final one take your operaiton volume and multiply by returned fee</returns>
        public decimal CalculateTransactionFee(string currencyPair, OperationTypes.OPERATION_TYPE operationType)
        {
            TransactionFee fee;

            if (_accountMonthlyVolume <= 0) //take highest possible
                fee = _fees.transaction.OrderBy(f => f.treshold).First();
            else
                fee = _fees.transaction.Where(f => f.treshold < _accountMonthlyVolume).OrderBy(f => f.treshold).First();

            if (fee == null)
                throw new InvalidDataException($"Fee not defined for operation type {operationType.ToString()} monthly volume {_accountMonthlyVolume.ToString()}");

            if (operationType == OperationTypes.OPERATION_TYPE.maker)
                return fee.maker;
            else if (operationType == OperationTypes.OPERATION_TYPE.taker)
                return fee.taker;
            else
                throw new InvalidDataException($"Incorrect operation type");
        }
    }
}