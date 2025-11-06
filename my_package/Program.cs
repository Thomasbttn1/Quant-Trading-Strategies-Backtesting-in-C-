using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace QuantBacktest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Backtest sur KFC & Starbucks (CSV utilisateur)");

            // 1) Chemins des datasets
            string kfcCsv = "Docs/KFC Dataset.csv";
            string sbuxCsv = "Docs/Starbucks Dataset.csv";
            if (!File.Exists(kfcCsv)) { Console.WriteLine($"Introuvable: {kfcCsv}"); return; }
            if (!File.Exists(sbuxCsv)) { Console.WriteLine($"Introuvable: {sbuxCsv}"); return; }

            // 2) Normaliser au format attendu
            string dataDir = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(dataDir);
            string kfcNorm = Path.Combine(dataDir, "KFC_normalized.csv");
            string sbuxNorm = Path.Combine(dataDir, "SBUX_normalized.csv");
            try { NormalizeCsvToExpectedFormat(kfcCsv, kfcNorm); Console.WriteLine($"CSV normalisé: {kfcNorm}"); }
            catch (Exception ex) { Console.WriteLine("Erreur normalisation KFC: " + ex.Message); return; }
            try { NormalizeCsvToExpectedFormat(sbuxCsv, sbuxNorm); Console.WriteLine($"CSV normalisé: {sbuxNorm}"); }
            catch (Exception ex) { Console.WriteLine("Erreur normalisation SBUX: " + ex.Message); return; }

            // 3) Charger les données
            var donneesKfc = new DonneesMarche();
            var donneesSbux = new DonneesMarche();
            try { donneesKfc.ChargerDepuisCSV(kfcNorm); }
            catch (Exception ex) { Console.WriteLine("Erreur chargement KFC: " + ex.Message); return; }
            try { donneesSbux.ChargerDepuisCSV(sbuxNorm); }
            catch (Exception ex) { Console.WriteLine("Erreur chargement SBUX: " + ex.Message); return; }

            // 4) Stratégie avec cooldown
            var strategie = new StrategieValue { MinIntervalEntreSignaux = TimeSpan.FromDays(60) };

            // 5) Backtests individuels
            var btKfc = new Backtester(strategie, donneesKfc) { RiskFreeRateAnnual = 0.02 };
            btKfc.Executer();
            Console.WriteLine("[KFC]");
            btKfc.AfficherResultats();
            Console.WriteLine($"Sharpe (KFC, annualisé) : {btKfc.ResultatPerformance.SharpeRatio:F2}");

            var btSbux = new Backtester(strategie, donneesSbux) { RiskFreeRateAnnual = 0.02 };
            btSbux.Executer();
            Console.WriteLine("[SBUX]");
            btSbux.AfficherResultats();
            Console.WriteLine($"Sharpe (SBUX, annualisé) : {btSbux.ResultatPerformance.SharpeRatio:F2}");

            // 6) Portefeuille KFC + SBUX
            var pf = new PortefeuilleBacktester(strategie);
            File.Copy(kfcNorm, Path.Combine(dataDir, "KFC.csv"), overwrite: true);
            File.Copy(sbuxNorm, Path.Combine(dataDir, "SBUX.csv"), overwrite: true);
            pf.ChargerDonnees(dataDir, new List<string> { "KFC", "SBUX" });
            pf.Executer();
            pf.AfficherResultatsGlobaux();

            Console.WriteLine("Backtest terminé.");
        }

        // Normalise un CSV hétérogène (en-têtes différents) vers: Date,Open,High,Low,Close,Volume
        private static void NormalizeCsvToExpectedFormat(string source, string dest)
        {
            using var sr = new StreamReader(source);
            string? header = sr.ReadLine();
            if (header == null) throw new InvalidDataException("CSV vide");

            char sep = header.Contains(';') && !header.Contains(',') ? ';' : ',';
            var cols = header.Split(sep, StringSplitOptions.TrimEntries);
            int idxDate = IndexOf(cols, new[] { "date", "timestamp" });
            int idxOpen = IndexOf(cols, new[] { "open", "open price", "ouverture" });
            int idxHigh = IndexOf(cols, new[] { "high", "haut" });
            int idxLow = IndexOf(cols, new[] { "low", "bas" });
            int idxClose = IndexOf(cols, new[] { "close", "adj close", "closing price", "clôture" });
            int idxVolume = IndexOf(cols, new[] { "volume", "vol" });

            if (idxDate < 0 || idxOpen < 0 || idxHigh < 0 || idxLow < 0 || idxClose < 0 || idxVolume < 0)
                throw new InvalidDataException("En-têtes CSV non reconnus. Attendu: Date/Open/High/Low/(Close ou Adj Close)/Volume");

            using var sw = new StreamWriter(dest);
            sw.WriteLine("Date,Open,High,Low,Close,Volume");

            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(sep);
                if (parts.Length <= Math.Max(Math.Max(Math.Max(idxDate, idxOpen), Math.Max(idxHigh, idxLow)), Math.Max(idxClose, idxVolume)))
                    continue;

                string dateS = parts[idxDate].Trim();
                string openS = parts[idxOpen].Trim();
                string highS = parts[idxHigh].Trim();
                string lowS = parts[idxLow].Trim();
                string closeS = parts[idxClose].Trim();
                string volS = parts[idxVolume].Trim();

                // Nettoyage éventuel des séparateurs de milliers
                openS = openS.Replace(" ", "").Replace("\u00A0", "");
                highS = highS.Replace(" ", "").Replace("\u00A0", "");
                lowS = lowS.Replace(" ", "").Replace("\u00A0", "");
                closeS = closeS.Replace(" ", "").Replace("\u00A0", "");
                volS = volS.Replace(" ", "").Replace("\u00A0", "");

                // Essayer différents formats de date
                string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "M/d/yyyy", "d/M/yyyy" };
                if (!DateTime.TryParseExact(dateS, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    // fallback: TryParse global
                    if (!DateTime.TryParse(dateS, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        continue;
                }

                if (!double.TryParse(openS, NumberStyles.Any, CultureInfo.InvariantCulture, out var open)) continue;
                if (!double.TryParse(highS, NumberStyles.Any, CultureInfo.InvariantCulture, out var high)) continue;
                if (!double.TryParse(lowS, NumberStyles.Any, CultureInfo.InvariantCulture, out var low)) continue;
                if (!double.TryParse(closeS, NumberStyles.Any, CultureInfo.InvariantCulture, out var close)) continue;
                if (!double.TryParse(volS, NumberStyles.Any, CultureInfo.InvariantCulture, out var vol)) continue;

                sw.WriteLine($"{date:yyyy-MM-dd},{open},{high},{low},{close},{vol}");
            }
        }

        private static int IndexOf(string[] headers, string[] candidates)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var h = headers[i].Trim().Trim('"').ToLowerInvariant();
                foreach (var c in candidates)
                {
                    if (h == c) return i;
                    // tolérance légère sur espaces/majuscules
                    if (h.Replace(" ", "") == c.Replace(" ", "")) return i;
                }
            }
            return -1;
        }
    }
}
