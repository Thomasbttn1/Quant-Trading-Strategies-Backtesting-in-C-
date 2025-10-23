using System;
using QuantBacktest;
using Xunit;

namespace my_package.Tests
{
    public class SignalTradingTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            var date = new DateTime(2020,1,1);
            var s = new SignalTrading(date, SignalTrading.TypeSignal.BUY, 100);
            Assert.Equal(date, s.Date);
            Assert.Equal(SignalTrading.TypeSignal.BUY, s.Type);
            Assert.Equal(100, s.Price);
            Assert.Equal(100, s.Prix);
        }

        [Fact]
        public void ToString_HasTypeAndPrice()
        {
            var s = new SignalTrading(new DateTime(2020,1,1), SignalTrading.TypeSignal.SELL, 150);
            var text = s.ToString();
            Assert.Contains("SELL", text);
            Assert.Contains("150", text);
        }
    }
}
