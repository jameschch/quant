using System;
using QuantConnect.Securities;
using QuantConnect.Orders;
using QuantConnect.Data.Market;
using QuantConnect;

namespace GeneticTree.RiskManagement
{
    public enum LotSize
    {
        Standard = 100000,
        Mini = 10000,
        Micro = 1000,
        Nano = 100,
    }

    public enum AgentAction
    {
        GoShort = -1,
        DoNothing = 0,
        GoLong = 1
    }

    public class FxRiskManagment
    {
        // Maximum equity proportion to put at risk in a single operation.
        private decimal _riskPerTrade;

        // Maximum equity proportion at risk in open positions in a given time.
        private decimal _maxExposure;

        // Maximum equity proportion at risk in a single trade.
        private decimal _maxExposurePerTrade;

        private int _lotSize;

        private int _minQuantity;

        private SecurityPortfolioManager _portfolio;

        /// <summary>
        /// Initializes a new instance of the <see cref="FxRiskManagment"/> class.
        /// </summary>
        /// <param name="portfolio">The QCAlgorithm Portfolio.</param>
        /// <param name="riskPerTrade">The max risk per trade.</param>
        /// <param name="maxExposurePerTrade">The maximum exposure per trade.</param>
        /// <param name="maxExposure">The maximum exposure in all trades.</param>
        /// <param name="lotsize">The minimum quantity to trade.</param>
        /// <exception cref="System.NotImplementedException">The pairs should be added to the algorithm before initialize the risk manager.</exception>
        public FxRiskManagment(SecurityPortfolioManager portfolio, decimal riskPerTrade, decimal maxExposurePerTrade,
                               decimal maxExposure, LotSize lotsize = LotSize.Micro, int minQuantity = 5)
        {
            _portfolio = portfolio;
            if (_portfolio.Securities.Count == 0)
            {
                throw new NotImplementedException("The pairs should be added to the algorithm before initialize the risk manager.");
            }
            this._riskPerTrade = riskPerTrade;
            _maxExposurePerTrade = maxExposurePerTrade;
            this._maxExposure = maxExposure;
            _lotSize = (int)lotsize;
            _minQuantity = minQuantity;
        }

        /// <summary>
        /// Calculates the entry orders and stop-loss price.
        /// </summary>
        /// <param name="pair">The Forex pair Symbol.</param>
        /// <param name="action">The order direction.</param>
        /// <returns>a Tuple with the quantity as Item1 and the stop-loss price as Item2. If quantity is zero, then means that no trade must be done.</returns>
        public Tuple<int, decimal, decimal> CalculateEntryOrders(QuoteBars data, Symbol pair, AgentAction action)
        {
            // If exposure is greater than the max exposure, then return zero.
            if (_portfolio.TotalMarginUsed > _portfolio.TotalPortfolioValue * _maxExposure)
            {
                return Tuple.Create(0, 0m, 0m);
            }
            var closePrice = _portfolio.Securities[pair].Price;
            var leverage = _portfolio.Securities[pair].Leverage;
            var exchangeRate = _portfolio.Securities[pair].QuoteCurrency.ConversionRate;
            var volatility = _portfolio.Securities[pair].VolatilityModel.Volatility;

            // Estimate the maximum entry order quantity given the risk per trade.
            var moneyAtRisk = _portfolio.TotalPortfolioValue * _riskPerTrade;
            var quantity = action == AgentAction.GoLong ? _lotSize : -_lotSize;
            if((volatility * exchangeRate)>0)
            {
                var maxQuantitybyRisk = moneyAtRisk / (volatility * exchangeRate);
                // Estimate the maximum entry order quantity given the exposure per trade.
                var maxBuySize = Math.Min(_portfolio.MarginRemaining, _portfolio.TotalPortfolioValue * _maxExposurePerTrade) * leverage;
                var maxQuantitybyExposure = maxBuySize / (closePrice * exchangeRate);
                // The final quantity is the lowest of both.
                quantity = (int)(Math.Round(Math.Min(maxQuantitybyRisk, maxQuantitybyExposure) / _lotSize, 0) * _lotSize);
                // If the final quantity is lower than the minimum quantity of the given lot size, then return zero.
                if (quantity < _lotSize * _minQuantity) return Tuple.Create(0, 0m, 0m);

                quantity = action == AgentAction.GoLong ? quantity : -quantity;
            }

            var stopLossPrice = closePrice + (action == AgentAction.GoLong ? -volatility : volatility);
            return Tuple.Create(quantity, stopLossPrice, action == AgentAction.GoLong ? data[pair].Ask.Close : data[pair].Bid.Close);
        }

        /// <summary>
        /// Updates the stop-loss price of all open StopMarketOrders.
        /// </summary>
        public void UpdateTrailingStopOrders(QuoteBars data)
        {
            // Get all the spot-loss orders.
            var openStopLossOrders = _portfolio.Transactions.GetOrderTickets(o => o.OrderType == OrderType.StopMarket && o.Status == OrderStatus.Submitted);
            foreach (var ticket in openStopLossOrders)
            {
                var stopLossPrice = ticket.SubmitRequest.StopPrice;
                var volatility = _portfolio.Securities[ticket.Symbol].VolatilityModel.Volatility;
                var actualPrice = _portfolio.Securities[ticket.Symbol].Price;
                // The StopLossOrder has the opposite direction of the original order.
                var originalOrderDirection = ticket.Quantity > 0 ? OrderDirection.Sell : OrderDirection.Buy;
                if (originalOrderDirection == OrderDirection.Sell)
                {
                    actualPrice = data[ticket.Symbol].Ask.Close;
                }
                if (originalOrderDirection == OrderDirection.Buy)
                {
                    actualPrice = data[ticket.Symbol].Bid.Close;
                }
                var newStopLossPrice = actualPrice + (volatility * (originalOrderDirection == OrderDirection.Buy ? -1 : 1));
                if ((originalOrderDirection == OrderDirection.Buy && newStopLossPrice > stopLossPrice)
                    || (originalOrderDirection == OrderDirection.Sell && newStopLossPrice < stopLossPrice))
                {

                    /*
                      if (originalOrderDirection == OrderDirection.Sell && data[ticket.Symbol].Ask.Close < System.Convert.ToDecimal(ticket.SubmitRequest.Tag))
                      {
                        if(newStopLossPrice>System.Convert.ToDecimal(ticket.SubmitRequest.Tag))
                        {
                            newStopLossPrice =  System.Convert.ToDecimal(ticket.SubmitRequest.Tag);
                        }
                      }
                      if (originalOrderDirection == OrderDirection.Buy && data[ticket.Symbol].Bid.Close > System.Convert.ToDecimal(ticket.SubmitRequest.Tag))
                      {
                        //price is on right direction
                        if(newStopLossPrice<System.Convert.ToDecimal(ticket.SubmitRequest.Tag))
                        {
                            newStopLossPrice =  System.Convert.ToDecimal(ticket.SubmitRequest.Tag);
                        }
                      }
                      */
                    ticket.Update(new UpdateOrderFields { Quantity = -(_portfolio[ticket.Symbol].Quantity), StopPrice = newStopLossPrice });
                }
                else if (!_portfolio[ticket.Symbol].Invested)
                {
                    ticket.Cancel();
                }
                else if(_portfolio[ticket.Symbol].Quantity!=ticket.Quantity)
                {
                    ticket.Update(new UpdateOrderFields { Quantity = -(_portfolio[ticket.Symbol].Quantity) });
     
                }

            }
        }
    }

}