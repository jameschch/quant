﻿using System;
using QuantConnect.Data;
using QuantConnect.Configuration;
using QuantConnect.Data.Consolidators;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Algorithm;
using QuantConnect;

namespace GeneticTree
{
    public partial class GeneticTreeAlgorithmExample : QCAlgorithm
    {
        private Rule _entry;
        private Rule _exit;
        private Symbol _symbol;
        private readonly bool IsOutOfSampleRun = true;
        private readonly int oosPeriod = 3;

        public override void Initialize()
        {
            SetCash(1000);
            SetStartDate(Config.GetValue<DateTime>("startDate", new DateTime(2017, 6, 12)));
            SetEndDate(Config.GetValue<DateTime>("endDate", new DateTime(2017, 7, 22)));

            if (IsOutOfSampleRun)
            {
                var startDate = new DateTime(year: 2016, month: 1, day: 1);
                SetStartDate(startDate);
                SetEndDate(startDate.AddMonths(oosPeriod));
                RuntimeStatistics["ID"] = GetParameter("ID");
                SetParameters(config.ToDictionary(k => k.Key, v => v.Value.ToString()));
            }

            _symbol = AddSecurity(SecurityType.Forex, "EURUSD", Resolution.Minute, Market.Oanda, false, 50m, false).Symbol;
            SetBrokerageModel(QuantConnect.Brokerages.BrokerageName.OandaBrokerage);
            var con = new TickConsolidator(new TimeSpan(1, 0, 0));

           // SetBenchmark(_symbol);

           

            var factory = new SignalFactory();

            _entry = factory.Create(this, _symbol, true, Resolution.Minute);
            _exit = factory.Create(this, _symbol, false, Resolution.Minute);

        }

        public override void OnData(Slice e)
        {
            if (!LiveMode && Portfolio.TotalPortfolioValue < 600)
            {
                Quit();
            }

            if (!_entry.IsReady()) return;
            if (!Portfolio.Invested && _entry.IsTrue())
            {
                SetHoldings(_symbol, 0.9m);
                Log("buy: " + Portfolio[_symbol].Price + " Portfolio:" + Portfolio.TotalPortfolioValue);
            }
            else if (_exit.IsTrue())
            {
                Liquidate();
                Log("liq: " + Portfolio[_symbol].Price + " Portfolio:" + Portfolio.TotalPortfolioValue);
            }
        }

        private static Dictionary<string, int> config = new Dictionary<string, int> {
            {"EntryIndicator1",  0},
            {"EntryIndicator2",  10},
            {"EntryIndicator3",  -1},
            {"EntryIndicator4",  2},
            {"EntryIndicator5",  3},
            {"EntryIndicator1Direction",  0},
            {"EntryIndicator2Direction",  0},
            {"EntryIndicator3Direction",  1},
            {"EntryIndicator4Direction",  0},
            {"EntryIndicator5Direction",  1},
            {"EntryOperator1",  0},
            {"EntryOperator2",  1},
            {"EntryOperator3",  0},
            {"EntryOperator4",  0},
            {"EntryRelationship1",  0},
            {"EntryRelationship2",  1},
            {"EntryRelationship3",  1},
            {"EntryRelationship4",  0},
            {"ExitIndicator1",  6},
            {"ExitIndicator2",  5},
            {"ExitIndicator3",  4},
            {"ExitIndicator4",  -1},
            {"ExitIndicator5",  2},
            {"ExitIndicator1Direction",  0},
            {"ExitIndicator2Direction",  0},
            {"ExitIndicator3Direction",  1},
            {"ExitIndicator4Direction",  1},
            {"ExitIndicator5Direction",  0},
            {"ExitOperator1",  0},
            {"ExitOperator2",  0},
            {"ExitOperator3",  0},
            {"ExitOperator4",  1},
            {"ExitRelationship1",  0},
            {"ExitRelationship2",  1},
            {"ExitRelationship3",  0},
            {"ExitRelationship4",  1},
            {"period",  1},
            {"slowPeriod",  2},
            {"fastPeriod",  3},
            {"signalPeriod",  4 }
        };
    }
}













































