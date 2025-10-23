using System;
namespace QuantBacktest
{
    public class Transaction
    {
        public DateTime DateEntree { get; private set; }
        public DateTime DateSortie { get; private set; }
        public double PrixEntree { get; private set; }
        public double PrixSortie { get; private set; }
        public SignalTrading.TypeSignal TypeInitial { get; private set; } // BUY ou SELL (short)
        public double Rendement => CalculerRendement();
        public double Profit => CalculerProfit();

        public double Quantite { get; private set; } = 1.0;
        public Transaction(DateTime entree, double prixEntree, DateTime sortie, double prixSortie, SignalTrading.TypeSignal typeInitial, double quantite = 1.0)
        {
            DateEntree = entree;
            DateSortie = sortie;
            PrixEntree = prixEntree;
            PrixSortie = prixSortie;
            TypeInitial = typeInitial;
            Quantite = quantite;
        }

        private double CalculerRendement()
        {
            if (TypeInitial == SignalTrading.TypeSignal.BUY)
                return (PrixSortie / PrixEntree) - 1.0;
            else
                return (PrixEntree / PrixSortie) - 1.0; // short
        }
        private double CalculerProfit()
        {
            return Rendement * PrixEntree * Quantite;
        }
        public override string ToString()
        {
            string sens = TypeInitial == SignalTrading.TypeSignal.BUY ? "Long" : "Short";
            string resultat = Rendement >= 0 ? "Gain" : "Perte";
            return $"{sens} du {DateEntree:yyyy-MM-dd} au {DateSortie:yyyy-MM-dd} → {resultat} : {Rendement:P2}";
        }
    }
}