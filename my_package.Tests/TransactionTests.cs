using System;
using QuantBacktest;
using Xunit;

namespace my_package.Tests
{
    public class TransactionTests
    {
        [Fact]
        public void Long_Rendement_Positive_When_Price_Increases()
        {
            var t = new Transaction(new DateTime(2020,1,1), 100, new DateTime(2020,2,1), 120, SignalTrading.TypeSignal.BUY);
            Assert.InRange(t.Rendement, 0.19, 0.21);
            Assert.True(t.Profit > 0);
        }

        [Fact]
        public void Short_Rendement_Positive_When_Price_Decreases()
        {
            var t = new Transaction(new DateTime(2020,1,1), 100, new DateTime(2020,2,1), 80, SignalTrading.TypeSignal.SELL);
            Assert.InRange(t.Rendement, 0.24, 0.26);
            Assert.True(t.Profit > 0);
        }
    }
}
