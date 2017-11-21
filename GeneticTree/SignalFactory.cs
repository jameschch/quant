﻿using System;
using System.Collections.Generic;
using QuantConnect.Configuration;
using QuantConnect.Indicators;
using QuantConnect;
using QuantConnect.Algorithm;
using GeneticTree.Signal;

namespace GeneticTree
{

    public class SignalFactory : AbstractSignalFactory
    {

        //todo: derive maximum signals from config keys
        private readonly int _maximumSignals = 5;
        int _period;
        int _slowPeriod;
        int _fastPeriod;
        int _signalPeriod;
        QCAlgorithm _algorithm;
        Resolution _resolution;
        private bool _enableParameterLog = false;

        public enum TechnicalIndicator
        {
            None = -1,
            SimpleMovingAverage = 0,
            MovingAverageConvergenceDivergence = 1,
            Stochastic = 2,
            RelativeStrengthIndex = 3,
            CommodityChannelIndex = 4,
            MomentumPercent = 5,
            WilliamsPercentR = 6,
            PercentagePriceOscillator = 7,
            AverageDirectionalIndex = 8,
            AverageTrueRange = 9,
            BollingerBands = 10,
            ExponentialMovingAverage = 11,
            ChannelBreakout=12
        }

        public override Rule Create(QCAlgorithm algorithm, Symbol symbol, bool isEntryRule, Resolution resolution = Resolution.Hour)
        {
            _algorithm = algorithm;
            _resolution = resolution;
            var entryOrExit = isEntryRule ? "Entry" : "Exit";

            _period = GetConfigValue("period");
            _slowPeriod = GetConfigValue("slowPeriod");
            _fastPeriod = GetConfigValue("fastPeriod");
            _signalPeriod = GetConfigValue("signalPeriod");

            ISignal parent = null;
            List<ISignal> list = new List<ISignal>();

            for (var i = 1; i <= _maximumSignals; i++)
            {
                var item = CreateIndicator(symbol, i, entryOrExit);
                if (parent != null)
                {
                    parent.Child = item;
                }

                //last item won't have operator
                if (i < _maximumSignals)
                {
                    var key = entryOrExit + "Operator" + i;
                    Operator op = (Operator)GetConfigValue(key);
                    item.Operator = op;
                }

                item.Parent = parent;
                parent = item;

                list.Add(item);
            }

            return new Rule(symbol, list);
        }

        protected override ISignal CreateIndicator(Symbol pair, int i, string entryOrExit)
        {
            var key = entryOrExit + "Indicator" + i + "Direction";
            var intDirection = GetConfigValue(key);

            var direction = intDirection == 0 ? Direction.LongOnly : Direction.ShortOnly;

            key = entryOrExit + "Indicator" + i;

            var indicator = (TechnicalIndicator)GetConfigValue(key);
            ISignal signal = null;

            switch (indicator)
            {
                case TechnicalIndicator.SimpleMovingAverage:
                    var fast = _algorithm.SMA(pair, _fastPeriod, _resolution);
                    var slow = _algorithm.SMA(pair, _slowPeriod, _resolution);
                    signal = new CrossingMovingAverageSignal(fast, slow, direction);
                    break;

                case TechnicalIndicator.ExponentialMovingAverage:
                    // Canonical cross moving average parameters.
                    var fastema = _algorithm.EMA(pair, period: 50);
                    var slowema = _algorithm.EMA(pair, period: 200);
                    signal = new CrossingMovingAverageSignal(fastema, slowema, direction);
                    break;

                case TechnicalIndicator.MovingAverageConvergenceDivergence:
                    var macd = _algorithm.MACD(pair, fastPeriod: 12, slowPeriod: 26, signalPeriod: 9, type:MovingAverageType.Simple, resolution:_resolution);
                    signal = new CrossingMovingAverageSignal(macd, macd.Signal, direction);
                    break;

                case TechnicalIndicator.Stochastic:
                    var sto = _algorithm.STO(pair, period: 14, resolution:_resolution);
                    signal = new OscillatorSignal(sto, new[] { 20, 80 }, direction);
                    break;

                case TechnicalIndicator.RelativeStrengthIndex:
                    var rsi = _algorithm.RSI(pair, period: 14);
                    signal = new OscillatorSignal(rsi, new[] { 30, 70 }, direction);
                    break;

                case TechnicalIndicator.CommodityChannelIndex:
                    var cci = _algorithm.CCI(pair, period: 20, movingAverageType:MovingAverageType.Simple, resolution:_resolution);
                    signal = new OscillatorSignal(cci, new[] { -100, 100 }, direction);
                    break;

                case TechnicalIndicator.MomentumPercent:
                    var pm = _algorithm.MOMP(pair, period: 60, resolution:_resolution);
                    signal = new OscillatorSignal(pm, new[] { -5, 5 }, direction);
                    break;

                case TechnicalIndicator.WilliamsPercentR:
                    var wr = _algorithm.WILR(pair, period: 14, resolution:_resolution);
                    signal = new OscillatorSignal(wr, new[] { -20, -80 }, direction);
                    break;

                case TechnicalIndicator.PercentagePriceOscillator:
                    var ppo = _algorithm.MACD(pair, fastPeriod: 12, slowPeriod: 26, signalPeriod: 9, type: MovingAverageType.Simple, resolution: _resolution)
                        .Over(_algorithm.EMA(pair, _period, resolution: _resolution)).Plus(constant: 100m);
                    var compound = new SimpleMovingAverage(_period).Of(ppo);
                    signal = new CrossingMovingAverageSignal(ppo, compound, direction);
                    break;

                case TechnicalIndicator.None:
                    signal = new EmptySignal();
                    break;

                case TechnicalIndicator.AverageDirectionalIndex:
                    var adx = _algorithm.ADX(pair, _period, _resolution);
                    signal = new OscillatorSignal(adx, new[] { 25, 25 }, direction);
                    break;

                    //todo:
                    //case TechnicalIndicator.AverageTrueRange:
                    //    var atr = _algorithm.ATR(pair, _period, MovingAverageType.Simple, _resolution);
                    //    signal = new OscillatorSignal(atr, oscillatorThresholds, direction);
                    //    break;

                case TechnicalIndicator.BollingerBands:
                    var bb = _algorithm.BB(pair, period: 20, k: 2);
                    signal = new BBOscillatorSignal(bb, direction);
                    break;

                case TechnicalIndicator.ChannelBreakout:
                    var delay = new Delay(5);
                    var _max = delay.Of(_algorithm.MAX(pair, 25));
                    var _min = delay.Of(_algorithm.MIN(pair, 25));
                    var cur = _algorithm.MAX(pair, 1);
                    signal = new ChannelOscillatorSignal(cur,_max, _min, direction);
                    break;
            }

            return signal;
        }

        protected override int GetConfigValue(string key)
        {
            int value;
            try
            {
                int.TryParse(_algorithm.GetParameter(key), out value);
                value = Config.GetInt(key, value);
                if (_enableParameterLog)
                {
                    _algorithm.Log(string.Format("Parameter {0} set to {1}", key, value));
                }
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentNullException(key,
                    "The gene " + key + " is not present either as Config or as Parameter");
            }

            return value;
        }
    }

    public abstract class AbstractSignalFactory
    {
        public abstract Rule Create(QCAlgorithm algorithm, Symbol symbol, bool isEntryRule, Resolution resolution = Resolution.Hour);
        protected abstract ISignal CreateIndicator(Symbol pair, int i, string entryOrExit);
        protected abstract int GetConfigValue(string key);
    }

}