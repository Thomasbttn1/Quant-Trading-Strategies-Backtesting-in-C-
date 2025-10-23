using System;
using QuantBacktest;
using Xunit;

namespace my_package.Tests
{
    public class CotationTests
    {
        [Fact]
        public void Constructor_Sets_all_fields()
        {
            var c = new Cotation(new DateTime(2021,5,6), 1, 2, 0.5, 1.5, 1000);
            Assert.Equal(new DateTime(2021,5,6), c.Date);
            Assert.Equal(1, c.Open);
            Assert.Equal(2, c.High);
            Assert.Equal(0.5, c.Low);
            Assert.Equal(1.5, c.Close);
            Assert.Equal(1000, c.Volume);
        }
    }
}
