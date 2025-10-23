using System;

namespace QuantBacktest
{
    public class Performance
    {
        public double RendementCumulatif { get; private set; }
        public double DrawdownMax { get; private set; }
        public double TauxReussite { get; private set; }
        public int NombreTransactions { get; private set; }
        public double SharpeRatio { get; private set; }

        public Performance(double rendement, double drawdown, double tauxReussite, int nbTransactions, double sharpeRatio = 0)
        {
            RendementCumulatif = rendement;
            DrawdownMax = drawdown;
            TauxReussite = tauxReussite;
            NombreTransactions = nbTransactions;
            SharpeRatio = sharpeRatio;
        }
        public override string ToString()
        {
            return $"Rendement total : {RendementCumulatif:P2}\n" +
                   $"Drawdown max : {DrawdownMax:P2}\n" +
                   $"Taux de réussite : {TauxReussite:F1}%\n" +
                   $"Sharpe (annualisé) : {SharpeRatio:F2}";
        }
    }
}