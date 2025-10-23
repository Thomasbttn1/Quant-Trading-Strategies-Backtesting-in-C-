using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantBacktest
{
    public class StrategieValue : StrategieTrading
    {
        private TimeSpan horizonEvaluation = TimeSpan.FromDays(252 * 2); // 2 ans
        // Cooldown minimal entre deux signaux pour limiter la fréquence
        public TimeSpan MinIntervalEntreSignaux { get; set; } = TimeSpan.FromDays(21); // ~1 mois de trading

        public StrategieValue() : base("Stratégie Value", autoriserShort: true) { }

        public override List<SignalTrading> GenererSignaux(DonneesMarche donnees)
        {
            var signaux = new List<SignalTrading>();
            if (donnees.NombreDeJours == 0)
                return signaux;

            DateTime? lastSignalDate = null;
            for (int i = 0; i < donnees.Cotations.Count; i++)
            {
                var actuelle = donnees.Cotations[i];
                var dateEvaluation = actuelle.Date - horizonEvaluation;

                var ancienne = donnees.Cotations
                    .Where(c => c.Date <= dateEvaluation)
                    .OrderByDescending(c => c.Date)
                    .FirstOrDefault();

                if (ancienne == null)
                    continue; // Pas assez de données historiques

                bool cooldownOk = lastSignalDate == null || (actuelle.Date - lastSignalDate.Value) >= MinIntervalEntreSignaux;
                if (!cooldownOk) continue;

                if (actuelle.Close < ancienne.Close)
                {
                    signaux.Add(new SignalTrading(actuelle.Date, SignalTrading.TypeSignal.BUY, actuelle.Close));
                    lastSignalDate = actuelle.Date;
                }
                else if (actuelle.Close > ancienne.Close && AutoriserShort)
                {
                    signaux.Add(new SignalTrading(actuelle.Date, SignalTrading.TypeSignal.SELL, actuelle.Close));
                    lastSignalDate = actuelle.Date;
                }
            }
            return signaux;
        }

        public override string Description()
        {
            return base.Description() + $", Horizon d'évaluation: {horizonEvaluation.Days / 252} ans, Min interval signaux: {MinIntervalEntreSignaux.Days} j";
        }
        
    }
}