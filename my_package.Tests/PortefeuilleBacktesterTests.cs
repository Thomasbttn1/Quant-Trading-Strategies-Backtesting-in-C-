using System;
using System.Collections.Generic;
using System.IO;
using QuantBacktest;
using Xunit;

namespace my_package.Tests
{
    public class PortefeuilleBacktesterTests
    {
        private static void WriteCsv(string path)
        {
            // 600 jours: 300 jours à 100 puis 300 jours à 90 pour déclencher des signaux
            var start = new DateTime(2016,1,1);
            using var sw = new StreamWriter(path);
            sw.WriteLine("Date,Open,High,Low,Close,Volume");
            for (int i=0; i<600; i++)
            {
                var d = start.AddDays(i);
                double close = i < 300 ? 100 : 90;
                sw.WriteLine($"{d:yyyy-MM-dd},1,1,1,{close},0");
            }
        }

        [Fact]
        public void ChargerDonnees_Executer_Aggregates_Per_Performance()
        {
            // Arrange dossier temporaire + 2 tickers
            string dir = Path.Combine(Path.GetTempPath(), "pfbt-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var tickers = new List<string> { "AAA", "BBB" };
                WriteCsv(Path.Combine(dir, "AAA.csv"));
                WriteCsv(Path.Combine(dir, "BBB.csv"));

                var strat = new StrategieValue();
                var pf = new PortefeuilleBacktester(strat);

                // Act
                pf.ChargerDonnees(dir, tickers);

                // Assert données chargées
                Assert.Equal(2, pf.DonneesParTicker.Count);
                Assert.True(pf.DonneesParTicker["AAA"].NombreDeJours > 0);
                Assert.True(pf.DonneesParTicker["BBB"].NombreDeJours > 0);

                // Exécution
                pf.Executer();

                Assert.Equal(2, pf.Backtests.Count);
                Assert.Equal(2, pf.Performances.Count);
                foreach (var kv in pf.Backtests)
                {
                    Assert.NotNull(kv.Value.ResultatPerformance);
                }

                // Sortie console globale
                var original = Console.Out;
                using var writer = new StringWriter();
                try
                {
                    Console.SetOut(writer);
                    pf.AfficherResultatsGlobaux();
                }
                finally
                {
                    Console.SetOut(original);
                }

                var output = writer.ToString();
                Assert.Contains("Résultats du portefeuille", output);
                Assert.Contains("[AAA]", output);
                Assert.Contains("[BBB]", output);
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* ignore cleanup errors */ }
            }
        }
    }
}
