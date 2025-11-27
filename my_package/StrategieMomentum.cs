using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantBacktest
{
    // Stratégie basée sur l'accélération du prix
    // Achète quand le prix s'accélère à la hausse, vend quand il s'accélère à la baisse
    public class StrategieMomentum : StrategieTrading
    {
        // Période pour calculer la vélocité (changement de prix)
        public int PeriodVelocite { get; set; } = 10; // jours
        
        // Période pour calculer l'accélération (changement de vélocité)
        public int PeriodAcceleration { get; set; } = 5; // jours
        
        // Seuils pour générer les signaux
        public double SeuilAccelPositif { get; set; } = 0.001; // 0.1%
        public double SeuilAccelNegatif { get; set; } = -0.001; // -0.1%
        
        // Cooldown minimal entre deux signaux
        public TimeSpan MinIntervalEntreSignaux { get; set; } = TimeSpan.FromDays(3);

        public StrategieMomentum() : base("Stratégie Momentum", autoriserShort: true) { }

        public override List<SignalTrading> GenererSignaux(DonneesMarche donnees)
        {
            var signaux = new List<SignalTrading>();
            
            if (donnees.NombreDeJours < PeriodVelocite + PeriodAcceleration)
                return signaux; // Pas assez de données

            // Calculer les vélocités (changement de prix)
            var velocites = new List<double>();
            for (int i = PeriodVelocite; i < donnees.Cotations.Count; i++)
            {
                double prixActuel = donnees.Cotations[i].Close;
                double prixAncien = donnees.Cotations[i - PeriodVelocite].Close;
                double velocite = (prixActuel - prixAncien) / prixAncien; // % change
                velocites.Add(velocite);
            }

            if (velocites.Count < PeriodAcceleration)
                return signaux; // Pas assez de données

            // Calculer les accélérations (changement de vélocité)
            DateTime? lastSignalDate = null;
            
            for (int i = PeriodAcceleration; i < velocites.Count; i++)
            {
                double velociteActuelle = velocites[i];
                double velociteAncienne = velocites[i - PeriodAcceleration];
                double acceleration = velociteActuelle - velociteAncienne;

                // Index correspondant dans les cotations
                int cotationIndex = i + PeriodVelocite;
                if (cotationIndex >= donnees.Cotations.Count)
                    break;

                var cotation = donnees.Cotations[cotationIndex];
                
                // Vérifier le cooldown
                bool cooldownOk = lastSignalDate == null || (cotation.Date - lastSignalDate.Value) >= MinIntervalEntreSignaux;
                if (!cooldownOk)
                    continue;

                // Générer les signaux basés sur l'accélération
                if (acceleration > SeuilAccelPositif)
                {
                    // Accélération positive : ACHETER
                    signaux.Add(new SignalTrading(cotation.Date, SignalTrading.TypeSignal.BUY, cotation.Close));
                    lastSignalDate = cotation.Date;
                }
                else if (acceleration < SeuilAccelNegatif && AutoriserShort)
                {
                    // Accélération négative : VENDRE (ou shorting)
                    signaux.Add(new SignalTrading(cotation.Date, SignalTrading.TypeSignal.SELL, cotation.Close));
                    lastSignalDate = cotation.Date;
                }
            }

            return signaux;
        }

        public override string Description()
        {
            return base.Description() 
                + $", Période Vélocité: {PeriodVelocite}j, Période Accélération: {PeriodAcceleration}j"
                + $", Seuils: +{SeuilAccelPositif:P1} / {SeuilAccelNegatif:P1}"
                + $", Min interval signaux: {MinIntervalEntreSignaux.Days}j";
        }
    }
}
