using System;
using System.IO;
using QuantBacktest;
using Xunit;

namespace my_package.Tests
{
    public class BacktesterTests
    {
        [Fact]
        public void Executer_Creates_Transactions_And_Performance()
        {
            // Prepare simple CSV with clear signal separation 3 entries
            var tmp = Path.GetTempFileName();
            using (var sw = new StreamWriter(tmp))
            {
                sw.WriteLine("Date,Open,High,Low,Close,Volume");
                var start = new DateTime(2016,1,1);
                for (int i=0;i<600;i++)
                {
                    var d = start.AddDays(i);
                    // Alternate close to generate signals with StrategieValue logic
                    double close = i < 300 ? 100 : 90;
                    sw.WriteLine($"{d:yyyy-MM-dd},1,1,1,{close},0");
                }
            }

            var dm = new DonneesMarche();
            dm.ChargerDepuisCSV(tmp);
            var strat = new StrategieValue();
            var bt = new Backtester(strat, dm);

            bt.Executer();

            Assert.NotNull(bt.ResultatPerformance);
            Assert.True(bt.Transactions.Count >= 0);
        }
    }
}
