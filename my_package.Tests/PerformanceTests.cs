using QuantBacktest;
using Xunit;

namespace my_package.Tests
{
    public class PerformanceTests
    {
        [Fact]
        public void ToString_Contains_all_metrics()
        {
            var p = new Performance(0.1234, 0.5, 60.0, 10, 1.23);
            var s = p.ToString();
            Assert.Contains("Rendement total", s);
            Assert.Contains("Drawdown max", s);
            Assert.Contains("Taux de réussite", s);
            Assert.Contains("Sharpe", s);
        }
    }
}
