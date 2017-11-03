using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using Exchange.MarketUtils;

namespace Exchange.Kraken
{
    public class OperationCostCalculator : IOperationFeesCalculator
    {
        //public enum TRANSFER_DIR { incomming, outgoing };
        //public enum FEE_TYPE { absolute, fraction };
        //public enum OPERATION_TYPE { maker, taker };

        private static Dictionary<string, SingleCurrencyFees> _fees;

        public OperationCostCalculator(string krakenPublicAPIaddress)
        {
            PublicApiConnector publicApiConnector = new PublicApiConnector(krakenPublicAPIaddress);
            string json = publicApiConnector.GetFees().Result;
            JObject j = JObject.Parse(json);

            JArray errors = (JArray)j["error"];
            if (errors.Count > 0)
            {
                string errMsg = string.Empty;

                foreach (var error in errors)
                    errMsg += error + Environment.NewLine;

                throw new System.Exception(errMsg);
            }

            if (_fees == null) //should not change during runtime
            {
                string feesString = j.SelectToken("result").ToString();
                _fees = JsonConvert.DeserializeObject<Dictionary<string, SingleCurrencyFees>>(feesString);
            }
        }

        public decimal CalculateTransferCost(string currency, OperationTypes.TRANSFER_DIR direction, decimal transferAmount = -1)
        {
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currencyPair"></param>
        /// <param name="operationType"></param>
        /// <param name="volumeUSD"></param>
        /// <returns>Fees as % eg. 0.5% => 0.005</returns>
        public decimal CalculateTransactionFee(string currencyPair, OperationTypes.OPERATION_TYPE operationType, decimal volumeUSD)
        {
            SingleCurrencyFees singleCurrencyFees;
            decimal fee = 1; //100% if we do not find any - this should prevent any transactions in case of error

            if (volumeUSD < 0)
                throw new InvalidDataException("Volume must be larger than 0");

            if (!_fees.TryGetValue(currencyPair, out singleCurrencyFees))
                throw new InvalidDataException($"Fees for pair {currencyPair} and operation {operationType.ToString()} not found");
            else
            {
                decimal[,] fees = null;

                if (operationType == OperationTypes.OPERATION_TYPE.maker)
                    fees = singleCurrencyFees.fees_maker;
                else if (operationType == OperationTypes.OPERATION_TYPE.taker)
                    fees = singleCurrencyFees.fees;
                else
                    throw new InvalidDataException("Invalid operation type");

                //select fee

                fee = fees[0,1]; //we assume sees are sorted

                for(int i = 0; i < fees.GetLength(0); i++)
                {
                    if (fees[i, 0] > volumeUSD)
                        break;
                    else
                        fee = fees[i, 1]/100;
                }
            }
            return fee;
        }
    }
}