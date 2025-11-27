using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantBacktest
{
    // Exécute le backtest d'une stratégie et calcule les performances
    public class Backtester
    {
        public StrategieTrading Strategie { get; private set; }
        public DonneesMarche Donnees { get; private set; }
        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public Performance ResultatPerformance { get; private set; } = new Performance(0, 0, 0, 0, 0);
    // Taux sans risque annuel (ex: 0.02 pour 2%). Utilisé pour le calcul de Sharpe.
    public double RiskFreeRateAnnual { get; set; } = 0.0;

        public Backtester(StrategieTrading strategie, DonneesMarche donnees)
        {
            Strategie = strategie;
            Donnees = donnees;
        }

        // Lance le backtest : génère signaux, crée transactions, calcule la performance
        public void Executer()
        {
            var signaux = Strategie.GenererSignaux(Donnees);
            Transactions.Clear();

            bool positionOuverte = false;
            SignalTrading? signalEntree = null;

            // Parcours chronologique des signaux
            foreach (var signal in signaux.OrderBy(s => s.Date))
            {
                if (!positionOuverte)
                {
                    // Ouverture d’une position
                    signalEntree = signal;
                    positionOuverte = true;
                }
                else
                {
                    // Fermeture d’une position (on prend le signal suivant comme sortie)
                    var transaction = new Transaction(
                        signalEntree!.Date,
                        signalEntree!.Prix,
                        signal.Date,
                        signal.Prix,
                        signalEntree!.Type);

                    Transactions.Add(transaction);
                    positionOuverte = false;
                }
            }

            // Calcul de la performance globale
            ResultatPerformance = CalculerPerformance();
        }

        // Calcule les indicateurs : rendement, drawdown, taux de réussite
        private Performance CalculerPerformance()
        {
            if (Transactions.Count == 0)
        return new Performance(0, 0, 0, 0, 0);

            double capital = 1.0; // capital de départ
            double maxCapital = 1.0;
            double drawdownMax = 0.0;
            int nbGagnantes = 0;

            foreach (var t in Transactions)
            {
                capital *= (1 + t.Rendement);

                if (t.Rendement > 0)
                    nbGagnantes++;

                // Mise à jour du drawdown
                double drawdown = (maxCapital - capital) / maxCapital;
                drawdownMax = Math.Max(drawdownMax, drawdown);
                maxCapital = Math.Max(maxCapital, capital);
            }

            double rendementTotal = capital - 1;
            double tauxReussite = (double)nbGagnantes / Transactions.Count * 100.0;

            // Calcul Sharpe sur rendements journaliers de l'equity (annualisé sqrt(252))
            double sharpe = 0.0;
            if (Donnees.Cotations.Count > 1)
            {
                // Préparer les fenêtres de transactions
                var intervals = Transactions.Select(t => new
                {
                    Start = t.DateEntree,
                    End = t.DateSortie,
                    Dir = t.TypeInitial == SignalTrading.TypeSignal.BUY ? 1 : -1
                }).ToList();

                var dailyReturns = new List<double>(Donnees.Cotations.Count);
                double rfDaily = Math.Pow(1.0 + RiskFreeRateAnnual, 1.0 / 252.0) - 1.0;
                for (int i = 1; i < Donnees.Cotations.Count; i++)
                {
                    var prev = Donnees.Cotations[i - 1];
                    var curr = Donnees.Cotations[i];
                    // Retour de l'actif
                    double assetRet = (curr.Close / prev.Close) - 1.0;
                    // Direction selon transaction active sur l'intervalle [prev.Date, curr.Date]
                    int dir = 0;
                    foreach (var iv in intervals)
                    {
                        if (iv.Start <= prev.Date && iv.End >= curr.Date)
                        {
                            dir = iv.Dir;
                            break;
                        }
                    }
                    double equityRet = dir * assetRet; // 0 si flat
                    double excessRet = equityRet - rfDaily; // rendement en excès du sans risque
                    if (!double.IsNaN(excessRet) && !double.IsInfinity(excessRet))
                        dailyReturns.Add(excessRet);
                }

                if (dailyReturns.Count > 1)
                {
                    double mean = 0.0; foreach (var r in dailyReturns) mean += r; mean /= dailyReturns.Count;
                    double var = 0.0; foreach (var r in dailyReturns) { double d = r - mean; var += d * d; }
                    var /= (dailyReturns.Count - 1);
                    double std = Math.Sqrt(var);
                    if (std > 0)
                    {
                        sharpe = (mean / std) * Math.Sqrt(252.0);
                    }
                }
            }

            return new Performance(rendementTotal, drawdownMax, tauxReussite, Transactions.Count, sharpe);
        }

        // Affiche un résumé des résultats
        public void AfficherResultats()
        {
            Console.WriteLine($"--- Résultats du Backtest : {Strategie.Nom} ---");
            Console.WriteLine($"Nombre de transactions : {Transactions.Count}");
            Console.WriteLine(ResultatPerformance);
            Console.WriteLine("----------------------------------------------");
        }
    }
}
