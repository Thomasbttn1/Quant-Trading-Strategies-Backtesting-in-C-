using System;
using System.Collections.Generic;
namespace QuantBacktest
{
    public abstract class StrategieTrading
    {
        public string Nom { get; protected set; }
        public bool AutoriserShort { get; protected set; } = false;

        protected StrategieTrading(string nom, bool autoriserShort = false)
        {
            Nom = nom;
            AutoriserShort = autoriserShort;
        }

        public abstract List<SignalTrading> GenererSignaux(DonneesMarche donnees);

        public virtual string Description()
        {
            return $"Stratégie: {Nom}, Autorise le short: {AutoriserShort}";
        }        
    }
}