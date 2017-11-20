using System;
using GeneticTree.RiskManagement;
using QuantConnect;
using QuantConnect.Algorithm;

namespace GeneticTree
{
    public class Configuration
    {
        public static decimal _leverage = 50m;

        // How much of the total strategy equity can be at risk as maximum in all trades.
        public static decimal _maxExposure = 0.8m;

        // How much of the total strategy equity can be at risk in a single trade.
        public static decimal _maxExposurePerTrade = 0.25m;

        // The max strategy equity proportion to put at risk in a single operation.
        public static decimal _riskPerTrade = 0.01m;

        public static decimal takeProfit = 0.05m;

        public static Resolution _resolution = Resolution.Minute;

        public static double accountsize = 10000;

        // Smallest lot
        public static LotSize _lotSize = LotSize.Nano;

        /// <summary>
        ///     Gets the gene int from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">The gene " + key + " is not present either as Config or as Parameter</exception>
        /// <remarks>
        ///     This method makes the algorithm working with the genes defined from the Config (as in the Lean Optimization) and
        ///     from the Parameters (as the Lean Caller).
        /// </remarks>
        public static decimal GetConfigDecimal(string key, decimal def, QCAlgorithm algo)
        {
            // I'll keep this line as in the original code. 
            //var gene = Config.GetValue<decimal>(key);
            var gene = decimal.MinValue;
            if (gene == decimal.MinValue)//not found in config, then get from parameter
            {
                try
                {
                    gene = decimal.Parse(algo.GetParameter(key));
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (ArgumentNullException e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    return def;
                }
            }
            return gene;
        }

        /// <summary>
        ///     Gets the gene int from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">The gene " + key + " is not present either as Config or as Parameter</exception>
        /// <remarks>
        ///     This method makes the algorithm working with the genes defined from the Config (as in the Lean Optimization) and
        ///     from the Parameters (as the Lean Caller).
        /// </remarks>
        public static DateTime GetConfigDateTime(string key, DateTime def, QCAlgorithm algo)
        {
            // I'll keep this line as in the original code. 
            //var gene = Config.GetValue<DateTime>(key, null);
            var gene = DateTime.MinValue;
            if (gene == DateTime.MinValue)//not found in config, then get from parameter
            {
                try
                {
                    gene = DateTime.Parse(algo.GetParameter(key));
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (ArgumentNullException e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    return def;
                }
            }
            return gene;
        }
    }
}
