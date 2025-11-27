using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuantBacktest
{
    // Gère le backtest sur plusieurs actifs en portefeuille
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

        // Charge les données CSV pour chaque titre
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

        // Exécute la stratégie sur tous les actifs du portefeuille
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

        // Affiche les résultats moyens du portefeuille
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
