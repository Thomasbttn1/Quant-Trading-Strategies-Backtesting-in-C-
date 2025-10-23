using System;
using System.IO;
using QuantBacktest;
using Xunit;

namespace my_package.Tests
{
    public class StrategieValueTests
    {
        [Fact]
        public void GenererSignaux_Returns_some_signals_when_sufficient_history()
        {
            var tmp = Path.GetTempFileName();
            // Two years apart prices to trigger BUY and SELL depending on trend
            var start = new DateTime(2018,1,1);
            using (var sw = new StreamWriter(tmp))
            {
                sw.WriteLine("Date,Open,High,Low,Close,Volume");
                for (int i=0;i<600;i++)
                {
                    var d = start.AddDays(i);
                    double close = i < 300 ? 100 : 90; // lower after 2 years to trigger BUY
                    sw.WriteLine($"{d:yyyy-MM-dd},1,1,1,{close},0");
                }
            }
            var dm = new DonneesMarche();
            dm.ChargerDepuisCSV(tmp);

            var strat = new StrategieValue();
            var signaux = strat.GenererSignaux(dm);

            Assert.NotNull(signaux);
            Assert.Contains(signaux, s => s.Type == SignalTrading.TypeSignal.BUY);
        }
    }
}
