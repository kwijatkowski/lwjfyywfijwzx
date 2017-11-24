using Exchange.MarketUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Exchange.Kraken
{
    public class OperationCostCalculator : IOperationFeesCalculator
    {
        //public enum TRANSFER_DIR { incomming, outgoing };
        //public enum FEE_TYPE { absolute, fraction };
        //public enum OPERATION_TYPE { maker, taker };

        private static Dictionary<string, SingleCurrencyFees> _fees;
        private decimal _accountMonthlyVolume;

        public OperationCostCalculator(string krakenPublicAPIaddress, decimal accountMonthlyVolume)
        {
            _accountMonthlyVolume = accountMonthlyVolume;
            PublicApiConnector publicApiConnector = new PublicApiConnector(krakenPublicAPIaddress);

            if (_fees == null) //should not change during runtime
            {
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

                string feesString = j.SelectToken("result").ToString();
                _fees = JsonConvert.DeserializeObject<Dictionary<string, SingleCurrencyFees>>(feesString);
            }
        }

        public decimal CalculateTransferCost(string currency, OperationTypes.TRANSFER_DIR direction, decimal transferAmount = -1)
        {
            string currencySymbol = CurrenciesNamesMap.MapNameToSymbol(currency);
            decimal fee = 0;

            if (direction == OperationTypes.TRANSFER_DIR.outgoing) //withdrawal
            {
                //https://support.kraken.com/hc/en-us/articles/201893608-What-are-the-withdrawal-fees-
                Dictionary<string, decimal> WithdrawalFees = new Dictionary<string, decimal>();
                WithdrawalFees.Add(KrakenCurrencies.XBT, new decimal(0.001));
                WithdrawalFees.Add(KrakenCurrencies.ETH, new decimal(0.005));
                WithdrawalFees.Add(KrakenCurrencies.XRP, new decimal(0.02));
                WithdrawalFees.Add(KrakenCurrencies.XLM, new decimal(0.00002));
                WithdrawalFees.Add(KrakenCurrencies.LTC, new decimal(0.02));
                WithdrawalFees.Add(KrakenCurrencies.XDG, new decimal(2.00));
                WithdrawalFees.Add(KrakenCurrencies.ZEC, new decimal(0.00010));
                WithdrawalFees.Add(KrakenCurrencies.ICN, new decimal(0.2));
                WithdrawalFees.Add(KrakenCurrencies.REP, new decimal(0.01));
                WithdrawalFees.Add(KrakenCurrencies.ETC, new decimal(0.005));
                WithdrawalFees.Add(KrakenCurrencies.MLN, new decimal(0.003));
                WithdrawalFees.Add(KrakenCurrencies.XMR, new decimal(0.05));
                WithdrawalFees.Add(KrakenCurrencies.DASH, new decimal(0.005));
                WithdrawalFees.Add(KrakenCurrencies.GNO, new decimal(0.01));
                WithdrawalFees.Add(KrakenCurrencies.USDT, new decimal(5));
                WithdrawalFees.Add(KrakenCurrencies.EOS, new decimal(0.50000));
                WithdrawalFees.Add(KrakenCurrencies.BCH, new decimal(0.001));
                //WithdrawalFees.Add(KrakenCurrencies., 0.01); //dash instant - not available

                if (WithdrawalFees.TryGetValue(currencySymbol, out fee))
                    return fee;
                else
                    throw new Exception($"Transfer fees not defined for {direction.ToString()} and currency {currency}");
            }
            else
            {
                //https://support.kraken.com/hc/en-us/articles/201396777
                Dictionary<string, decimal> DepositFees = new Dictionary<string, decimal>();
                DepositFees.Add(KrakenCurrencies.XBT, 0);
                DepositFees.Add(KrakenCurrencies.LTC, 0);
                DepositFees.Add(KrakenCurrencies.XDG, 0);
                DepositFees.Add(KrakenCurrencies.XRP, 0);
                DepositFees.Add(KrakenCurrencies.XLM, 0);
                DepositFees.Add(KrakenCurrencies.ETH, 0);
                DepositFees.Add(KrakenCurrencies.ETC, 0);
                DepositFees.Add(KrakenCurrencies.MLN, new decimal(0.01));
                DepositFees.Add(KrakenCurrencies.XMR, 0);
                DepositFees.Add(KrakenCurrencies.REP, 0);
                DepositFees.Add(KrakenCurrencies.ICN, 0);
                DepositFees.Add(KrakenCurrencies.ZEC, 0);
                DepositFees.Add(KrakenCurrencies.DASH, 0);
                DepositFees.Add(KrakenCurrencies.GNO, 0);
                DepositFees.Add(KrakenCurrencies.USDT, new decimal(0.004));
                DepositFees.Add(KrakenCurrencies.EOS, new decimal(0.2));
                DepositFees.Add(KrakenCurrencies.BCH, 0);

                if (DepositFees.TryGetValue(currencySymbol, out fee))
                    return fee;
                else
                    throw new Exception($"Transfer fees not defined for {direction.ToString()} and currency {currency}");
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="currencyPair"></param>
        /// <param name="operationType"></param>
        /// <returns>Fees as % eg. 0.5% => 0.005</returns>
        public decimal CalculateTransactionFee(string currencyPair, OperationTypes.OPERATION_TYPE operationType)
        {
            SingleCurrencyFees singleCurrencyFees;
            decimal fee = 1; //100% if we do not find any - this should prevent any transactions in case of error

            if (_accountMonthlyVolume < 0)
                throw new InvalidDataException("Volume must be larger than 0");

            if (!_fees.TryGetValue(currencyPair, out singleCurrencyFees))
                throw new InvalidDataException($"Transaction fees for pair {currencyPair} and operation {operationType.ToString()} not found");
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

                fee = fees[0, 1]; //we assume sees are sorted

                for (int i = 0; i < fees.GetLength(0); i++)
                {
                    if (fees[i, 0] > _accountMonthlyVolume)
                        break;
                    else
                        fee = fees[i, 1] / 100;
                }
            }
            return fee;
        }
    }
}