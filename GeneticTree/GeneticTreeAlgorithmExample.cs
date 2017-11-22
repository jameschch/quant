using System;
using QuantConnect.Data;
using QuantConnect.Configuration;
using QuantConnect.Data.Consolidators;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Algorithm;
using QuantConnect;
using GeneticTree.RiskManagement;
using QuantConnect.Data.Market;

namespace GeneticTree
{
    public partial class GeneticTreeAlgorithmExample : QCAlgorithm
    {
        private readonly bool IsOutOfSampleRun = true;
        private readonly int oosPeriod = 3;
        public bool verbose = false;

        private List<Rule> _entry;
        private List<Rule> _exit;
        public List<Symbol> _symbols;
        private Dictionary<Symbol, bool> tookPartialProfit = new Dictionary<QuantConnect.Symbol, bool>();
        FxRiskManagment RiskManager;

        public override void Initialize()
        {
            SetCash(10000);
            SetStartDate(Configuration.GetConfigDateTime("startDate", new DateTime(2017, 1, 12), this));
            SetEndDate(Configuration.GetConfigDateTime("endDate", new DateTime(2017, 10, 13), this));
            Configuration.GetConfiguration("http://www.mocky.io/v2/5a15c8f42e00005200eab80e",config);
            if (IsOutOfSampleRun)
            {
                //var startDate = new DateTime(year: 2016, month: 1, day: 1);
                //SetStartDate(startDate);
                //SetEndDate(startDate.AddMonths(oosPeriod));
                RuntimeStatistics["ID"] = GetParameter("ID");
                SetParameters(config.ToDictionary(k => k.Key, v => v.Value.ToString()));
            }

            SetBrokerageModel(QuantConnect.Brokerages.BrokerageName.OandaBrokerage);
            var con = new TickConsolidator(new TimeSpan(1, 0, 0));

           // SetBenchmark(_symbol);
           
            _symbols = new List<Symbol>();
            _entry = new List<Rule>();
            _exit = new List<Rule>();
            foreach (var symbol in TradingSymbols.OandaFXMajors0)
            {
                var security= AddSecurity(SecurityType.Forex, symbol, Configuration._resolution, Market.Oanda, true, Configuration._leverage, false);
                _symbols.Add(security.Symbol);
            }
            foreach (var symbol in TradingSymbols.OandaCFD)
            {
                //    AddSecurity(SecurityType.Cfd, symbol, _resolution, Market.Oanda, true, _leverage, false);
            }
            var factory = new SignalFactory();
            foreach (var symbol in _symbols)
            {
                Securities[symbol].VolatilityModel = new ThreeSigmaVolatilityModel(STD(symbol: symbol, period: 12 * 60, resolution: Configuration._resolution), 20.0m);

                _entry.Add(factory.Create(this, symbol, true, Configuration._resolution));
                _exit.Add(factory.Create(this, symbol, false, Configuration._resolution));
            }

            RiskManager = new FxRiskManagment(Portfolio, Configuration._riskPerTrade, Configuration._maxExposurePerTrade, Configuration._maxExposure, Configuration._lotSize);

        }

        public void OnData(QuoteBars data)
        {
            foreach (var entry in _entry)
            {
                if (entry.IsReady())
                {
                    EntrySignal(data, entry);
                }
            }
            foreach (var entry in _exit)
            {
                if (entry.IsReady())
                {
                    ExitSignal(entry);
                }
            }
        }

        public void ExitSignal(Rule signal)
        {

            if (verbose && signal.IsTrue())
            {
                Log(string.Format("signal symbol:: {0}", signal.Symbol));
            }
            if (Portfolio[signal.Symbol].Invested && signal.IsTrue())
            {
                Liquidate(signal.Symbol);
            }
            else if (Portfolio[signal.Symbol].Invested && 
                     Portfolio[signal.Symbol].UnrealizedProfitPercent > Configuration.takeProfit &&
                     !tookPartialProfit[signal.Symbol])
            {
                //safeguard profits, 
                //liquidate half
                tookPartialProfit[signal.Symbol] = true;
                MarketOrder(signal.Symbol, -(Portfolio[signal.Symbol].Quantity / 2));
            }
        }

        public void EntrySignal(QuoteBars data, Rule signal)
        {

            if (verbose && signal.IsTrue())
            {
                Log(string.Format("signal symbol:: {0}", signal.Symbol));
            }
            if (!Portfolio[signal.Symbol].Invested)
            {
                if (signal.IsTrue())
                {
                    var openPrice = Securities[signal.Symbol].Price;
                    var entryValues = RiskManager.CalculateEntryOrders(data, signal.Symbol, AgentAction.GoLong);
                    if (entryValues.Item1 != 0)
                    {
                        tookPartialProfit[signal.Symbol] = false;
                        var ticket = MarketOrder(signal.Symbol, entryValues.Item1);
                        StopMarketOrder(signal.Symbol, -entryValues.Item1, entryValues.Item2, tag: entryValues.Item3.ToString("0.000000"));
                        if (verbose)
                        {
                            Log(string.Format("MarketOrder:: {0} {1}", signal.Symbol, entryValues.Item1));
                        }
                    }
                    //MarketOrder(signal.Symbol, size, false, "");
                }
            }
        }
        private static Dictionary<string, int> config = new Dictionary<string, int> {
            {"period",  1},
            {"slowPeriod",  200},
            {"fastPeriod",  20},
            {"signalPeriod",  4 }
        };

        /*
        private static Dictionary<string, int> config = new Dictionary<string, int> {
            {"EntryIndicator1",  0},
            {"EntryIndicator2",  10},
            {"EntryIndicator3",  0},
            {"EntryIndicator4",  2},
            {"EntryIndicator5",  3},
            {"EntryIndicator1Direction",  1},
            {"EntryIndicator2Direction",  1},
            {"EntryIndicator3Direction",  1},
            {"EntryIndicator4Direction",  1},
            {"EntryIndicator5Direction",  1},
            {"EntryOperator1",  1},
            {"EntryOperator2",  1},
            {"EntryOperator3",  1},
            {"EntryOperator4",  1},
            {"EntryRelationship1",  0},
            {"EntryRelationship2",  1},
            {"EntryRelationship3",  1},
            {"EntryRelationship4",  0},
            {"ExitIndicator1",  6},
            {"ExitIndicator2",  5},
            {"ExitIndicator3",  4},
            {"ExitIndicator4",  0},
            {"ExitIndicator5",  2},
            {"ExitIndicator1Direction",  0},
            {"ExitIndicator2Direction",  0},
            {"ExitIndicator3Direction",  0},
            {"ExitIndicator4Direction",  0},
            {"ExitIndicator5Direction",  0},
            {"ExitOperator1",  1},
            {"ExitOperator2",  1},
            {"ExitOperator3",  1},
            {"ExitOperator4",  1},
            {"ExitRelationship1",  0},
            {"ExitRelationship2",  1},
            {"ExitRelationship3",  0},
            {"ExitRelationship4",  1},
            {"period",  1},
            {"slowPeriod",  200},
            {"fastPeriod",  20},
            {"signalPeriod",  4 }
        };
        */
    }
}