using System;
using System.IO;
using QuantBacktest;
using Xunit;

namespace my_package.Tests
{
    public class StrategieMomentumTests
    {
        [Fact]
        public void GenererSignaux_Returns_buy_signal_when_acceleration_positive()
        {
            var tmp = Path.GetTempFileName();
            var start = new DateTime(2020, 1, 1);
            
            using (var sw = new StreamWriter(tmp))
            {
                sw.WriteLine("Date,Open,High,Low,Close,Volume");
                
                // Première phase : prix stable (accélération ~0)
                for (int i = 0; i < 20; i++)
                {
                    var d = start.AddDays(i);
                    sw.WriteLine($"{d:yyyy-MM-dd},100,100,100,100,1000");
                }
                
                // Deuxième phase : prix augmente progressivement (accélération positive)
                for (int i = 20; i < 40; i++)
                {
                    var d = start.AddDays(i);
                    double close = 100 + (i - 20) * 0.5; // +0.5% par jour
                    sw.WriteLine($"{d:yyyy-MM-dd},{close},{close},{close},{close},1000");
                }
            }
            
            var dm = new DonneesMarche();
            dm.ChargerDepuisCSV(tmp);
            
            var strat = new StrategieMomentum();
            var signaux = strat.GenererSignaux(dm);
            
            Assert.NotNull(signaux);
            Assert.Contains(signaux, s => s.Type == SignalTrading.TypeSignal.BUY);
            File.Delete(tmp);
        }

        [Fact]
        public void GenererSignaux_Returns_sell_signal_when_acceleration_negative()
        {
            var tmp = Path.GetTempFileName();
            var start = new DateTime(2020, 1, 1);
            
            using (var sw = new StreamWriter(tmp))
            {
                sw.WriteLine("Date,Open,High,Low,Close,Volume");
                
                // Première phase : prix à 150 (stable)
                for (int i = 0; i < 20; i++)
                {
                    var d = start.AddDays(i);
                    sw.WriteLine($"{d:yyyy-MM-dd},150,150,150,150,1000");
                }
                
                // Deuxième phase : prix baisse progressivement (accélération négative)
                for (int i = 20; i < 40; i++)
                {
                    var d = start.AddDays(i);
                    double close = 150 - (i - 20) * 0.5; // -0.5% par jour
                    sw.WriteLine($"{d:yyyy-MM-dd},{close},{close},{close},{close},1000");
                }
            }
            
            var dm = new DonneesMarche();
            dm.ChargerDepuisCSV(tmp);
            
            var strat = new StrategieMomentum();
            var signaux = strat.GenererSignaux(dm);
            
            Assert.NotNull(signaux);
            Assert.Contains(signaux, s => s.Type == SignalTrading.TypeSignal.SELL);
            File.Delete(tmp);
        }

        [Fact]
        public void GenererSignaux_Returns_empty_when_insufficient_data()
        {
            var tmp = Path.GetTempFileName();
            var start = new DateTime(2020, 1, 1);
            
            using (var sw = new StreamWriter(tmp))
            {
                sw.WriteLine("Date,Open,High,Low,Close,Volume");
                // Seulement 5 jours (pas assez pour calculer l'accélération)
                for (int i = 0; i < 5; i++)
                {
                    var d = start.AddDays(i);
                    sw.WriteLine($"{d:yyyy-MM-dd},100,100,100,100,1000");
                }
            }
            
            var dm = new DonneesMarche();
            dm.ChargerDepuisCSV(tmp);
            
            var strat = new StrategieMomentum();
            var signaux = strat.GenererSignaux(dm);
            
            Assert.NotNull(signaux);
            Assert.Empty(signaux);
            File.Delete(tmp);
        }

        [Fact]
        public void GenererSignaux_Respects_cooldown_interval()
        {
            var tmp = Path.GetTempFileName();
            var start = new DateTime(2020, 1, 1);
            
            using (var sw = new StreamWriter(tmp))
            {
                sw.WriteLine("Date,Open,High,Low,Close,Volume");
                
                // Données avec oscillations rapides (accélération alternée)
                for (int i = 0; i < 100; i++)
                {
                    var d = start.AddDays(i);
                    double close = 100 + (Math.Sin(i * 0.5) * 5); // oscillation
                    sw.WriteLine($"{d:yyyy-MM-dd},{close},{close},{close},{close},1000");
                }
            }
            
            var dm = new DonneesMarche();
            dm.ChargerDepuisCSV(tmp);
            
            var strat = new StrategieMomentum { MinIntervalEntreSignaux = TimeSpan.FromDays(10) };
            var signaux = strat.GenererSignaux(dm);
            
            // Vérifier que les signaux respectent le cooldown
            Assert.NotNull(signaux);
            for (int i = 0; i < signaux.Count - 1; i++)
            {
                var diff = signaux[i + 1].Date - signaux[i].Date;
                Assert.True(diff >= strat.MinIntervalEntreSignaux, 
                    $"Signaux générés sans respecter le cooldown: {diff.Days} jours");
            }
            
            File.Delete(tmp);
        }

        [Fact]
        public void Description_Contains_strategy_name()
        {
            var strat = new StrategieMomentum();
            var desc = strat.Description();
            
            Assert.Contains("Momentum", desc);
            Assert.Contains("Vélocité", desc);
            Assert.Contains("Accélération", desc);
        }
    }
}
