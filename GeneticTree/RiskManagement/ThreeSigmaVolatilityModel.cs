using System;
using System.Linq;
using QuantConnect.Securities;
using QuantConnect.Data;
using QuantConnect.Indicators;
using System.Collections.Generic;
namespace GeneticTree.RiskManagement
{
    /// <summary>
    /// Provides an implementation of <see cref="IVolatilityModel"/> that computes the
    /// relative standard deviation as the volatility of the security
    /// </summary>
    public class ThreeSigmaVolatilityModel : IVolatilityModel
    {
        private readonly TimeSpan _periodSpan;
        private StandardDeviation _standardDeviation;
        private decimal _percentage;

        /// <summary>
        /// Gets the volatility of the security as a percentage
        /// </summary>
        public decimal Volatility
        {
            get { return _standardDeviation * _percentage; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantConnect.Securities.RelativeStandardDeviationVolatilityModel"/> class
        /// </summary>
        /// <param name="periodSpan">The time span representing one 'period' length</param>
        /// <param name="periods">The nuber of 'period' lengths to wait until updating the value</param>
        public ThreeSigmaVolatilityModel(StandardDeviation standardDeviation, decimal percentage = 2.5m)
        {
            _standardDeviation = standardDeviation;
            _percentage = percentage;
            _periodSpan = TimeSpan.FromMinutes(standardDeviation.Period);
        }

        /// <summary>
        /// Updates this model using the new price information in
        /// the specified security instance
        /// </summary>
        /// <param name="security">The security to calculate volatility for</param>
        /// <param name="data"></param>
        public void Update(Security security, BaseData data)
        {
        }

        public IEnumerable<HistoryRequest> GetHistoryRequirements(Security security, DateTime utcTime)
        {
            return Enumerable.Empty<HistoryRequest>();
        }
    }
}