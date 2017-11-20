using System;
using GeneticTree.RiskManagement;
using QuantConnect;

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

    }
}
