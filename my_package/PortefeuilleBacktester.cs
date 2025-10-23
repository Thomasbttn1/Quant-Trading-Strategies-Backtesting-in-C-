using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuantBacktest
{
    /// <summary>
    /// Gère l’exécution d’une stratégie sur plusieurs actifs (portefeuille).
    /// </summary>
    public class PortefeuilleBacktester
    {
        public StrategieTrading Strategie { get; private set; }
        public Dictionary<string, DonneesMarche> DonneesParTicker { get; private set; } = new();
        public Dictionary<string, Backtester> Backtests { get; private set; } = new();
        public List<Performance> Performances { get; private set; } = new();

        public PortefeuilleBacktester(StrategieTrading strategie)
        {
            Strategie = strategie;
        }

        /// <summary>
        /// Charge les données de plusieurs fichiers CSV correspondant à différents tickers.
        /// </summary>
        public void ChargerDonnees(string dossierData, List<string> tickers)
        {
            foreach (var ticker in tickers)
            {
                string chemin = Path.Combine(dossierData, $"{ticker}.csv");

                var donnees = new DonneesMarche();
                donnees.ChargerDepuisCSV(chemin);

                DonneesParTicker[ticker] = donnees;
            }
        }

        /// <summary>
        /// Exécute la stratégie sur chaque actif du portefeuille.
        /// </summary>
        public void Executer()
        {
            Backtests.Clear();
            Performances.Clear();

            foreach (var kvp in DonneesParTicker)
            {
                string ticker = kvp.Key;
                var donnees = kvp.Value;

                var backtester = new Backtester(Strategie, donnees)
                {
                    RiskFreeRateAnnual = 0.02 // même taux sans risque que dans Program.cs
                };
                backtester.Executer();

                Backtests[ticker] = backtester;
                Performances.Add(backtester.ResultatPerformance);
            }
        }

        /// <summary>
        /// Calcule la performance moyenne et globale du portefeuille.
        /// </summary>
        public void AfficherResultatsGlobaux()
        {
            Console.WriteLine("=== Résultats du portefeuille ===");
            double rendementMoyen = Performances.Average(p => p.RendementCumulatif);
            double drawdownMoyen = Performances.Average(p => p.DrawdownMax);
            double tauxReussiteMoyen = Performances.Average(p => p.TauxReussite);
            double sharpeMoyen = Performances.Average(p => p.SharpeRatio);

            Console.WriteLine($"Rendement moyen : {rendementMoyen:P2}");
            Console.WriteLine($"Drawdown moyen : {drawdownMoyen:P2}");
            Console.WriteLine($"Taux de réussite moyen : {tauxReussiteMoyen:F1}%");
            Console.WriteLine($"Sharpe moyen (annualisé) : {sharpeMoyen:F2}");
            Console.WriteLine("--------------------------------");

            foreach (var kvp in Backtests)
            {
                Console.WriteLine($"[{kvp.Key}]");
                kvp.Value.AfficherResultats();
            }
        }
    }
}
