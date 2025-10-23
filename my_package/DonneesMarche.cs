using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace QuantBacktest
{
    public class DonneesMarche
    {
        public List<Cotation> Cotations { get; private set; } = new List<Cotation>();

        public DonneesMarche() { }

        /// <summary>
        /// Charge les données de marché à partir d’un fichier CSV.
        /// Format attendu : Date,Open,High,Low,Close,Volume
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier CSV</param>
        public void ChargerDepuisCSV(string cheminFichier)
        {
            if (!File.Exists(cheminFichier))
                throw new FileNotFoundException("Le fichier CSV spécifié est introuvable.", cheminFichier);

            Cotations.Clear(); // Réinitialiser la liste si on recharge

            using (var lecteur = new StreamReader(cheminFichier))
            {
                string ligne;
                bool premiereLigne = true;

                while ((ligne = lecteur.ReadLine()) != null)
                {
                    // Ignorer l’entête
                    if (premiereLigne)
                    {
                        premiereLigne = false;
                        continue;
                    }

                    var valeurs = ligne.Split(',');

                    if (valeurs.Length < 6)
                        continue; // Ligne invalide → on passe

                    try
                    {
                        // Tenter plusieurs formats de date courants
                        string[] formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/M/d", "d/M/yyyy" };
                        DateTime date = DateTime.ParseExact(valeurs[0].Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
                        double open = double.Parse(valeurs[1], CultureInfo.InvariantCulture);
                        double high = double.Parse(valeurs[2], CultureInfo.InvariantCulture);
                        double low = double.Parse(valeurs[3], CultureInfo.InvariantCulture);
                        double close = double.Parse(valeurs[4], CultureInfo.InvariantCulture);
                        double volume = double.Parse(valeurs[5], CultureInfo.InvariantCulture);

                        Cotation c = new Cotation(date, open, high, low, close, volume);
                        Cotations.Add(c);
                    }
                    catch (FormatException)
                    {
                        // Ligne mal formée → on ignore
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Retourne le nombre total de cotations chargées.
        /// </summary>
        public int NombreDeJours => Cotations.Count;

        /// <summary>
        /// Retourne la première date disponible dans les données.
        /// </summary>
        public DateTime DateDebut => Cotations.Count > 0 ? Cotations[0].Date : DateTime.MinValue;

        /// <summary>
        /// Retourne la dernière date disponible dans les données.
        /// </summary>
        public DateTime DateFin => Cotations.Count > 0 ? Cotations[^1].Date : DateTime.MinValue;

        public override string ToString()
        {
            return $"Données de marché : {NombreDeJours} jours de {DateDebut:yyyy-MM-dd} à {DateFin:yyyy-MM-dd}";
        }
    }
}
