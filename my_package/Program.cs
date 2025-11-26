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

            // 1) Chemins des datasets - construire depuis la racine du repo
            // AppContext.BaseDirectory est bin/Debug/net6.0, donc remonter 3 niveaux
            string projectRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
            projectRoot = Path.GetFullPath(projectRoot);
            string kfcCsv = Path.Combine(projectRoot, "Docs", "KFC Dataset.csv");
            string sbuxCsv = Path.Combine(projectRoot, "Docs", "Starbucks Dataset.csv");
            string nflxCsv = Path.Combine(projectRoot, "Docs", "nflx_2014_2023.csv");
            if (!File.Exists(kfcCsv)) { Console.WriteLine($"Introuvable: {kfcCsv}"); return; }
            if (!File.Exists(sbuxCsv)) { Console.WriteLine($"Introuvable: {sbuxCsv}"); return; }
            if (!File.Exists(nflxCsv)) { Console.WriteLine($"Introuvable: {nflxCsv}"); return; }

            // 2) Normaliser au format attendu
            string dataDir = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(dataDir);
            string kfcNorm = Path.Combine(dataDir, "KFC_normalized.csv");
            string sbuxNorm = Path.Combine(dataDir, "SBUX_normalized.csv");
            string nflxNorm = Path.Combine(dataDir, "NFLX_normalized.csv");
            try { NormalizeCsvToExpectedFormat(kfcCsv, kfcNorm); Console.WriteLine($"CSV normalisé: {kfcNorm}"); }
            catch (Exception ex) { Console.WriteLine("Erreur normalisation KFC: " + ex.Message); return; }
            try { NormalizeCsvToExpectedFormat(sbuxCsv, sbuxNorm); Console.WriteLine($"CSV normalisé: {sbuxNorm}"); }
            catch (Exception ex) { Console.WriteLine("Erreur normalisation SBUX: " + ex.Message); return; }
            try { NormalizeCsvToExpectedFormat(nflxCsv, nflxNorm); Console.WriteLine($"CSV normalisé: {nflxNorm}"); }
            catch (Exception ex) { Console.WriteLine("Erreur normalisation NFLX: " + ex.Message); return; }

            // 3) Charger les données
            var donneesKfc = new DonneesMarche();
            var donneesSbux = new DonneesMarche();
            var donneesNflx = new DonneesMarche();
            try { donneesKfc.ChargerDepuisCSV(kfcNorm); }
            catch (Exception ex) { Console.WriteLine("Erreur chargement KFC: " + ex.Message); return; }
            try { donneesSbux.ChargerDepuisCSV(sbuxNorm); }
            catch (Exception ex) { Console.WriteLine("Erreur chargement SBUX: " + ex.Message); return; }
            try { donneesNflx.ChargerDepuisCSV(nflxNorm); }
            catch (Exception ex) { Console.WriteLine("Erreur chargement NFLX: " + ex.Message); return; }

            // 4) Stratégies
            var strategieValue = new StrategieValue { MinIntervalEntreSignaux = TimeSpan.FromDays(60) };
            var strategieMomentum = new StrategieMomentum 
            { 
                PeriodVelocite = 10, 
                PeriodAcceleration = 5,
                SeuilAccelPositif = 0.001,
                SeuilAccelNegatif = -0.001,
                MinIntervalEntreSignaux = TimeSpan.FromDays(3)
            };

            // ============================================
            // 5) BACKTESTS AVEC STRATÉGIE VALUE
            // ============================================
            Console.WriteLine("\n=== BACKTESTS STRATÉGIE VALUE ===\n");
            
            var btKfcValue = new Backtester(strategieValue, donneesKfc) { RiskFreeRateAnnual = 0.02 };
            btKfcValue.Executer();
            Console.WriteLine("[KFC - Value]");
            btKfcValue.AfficherResultats();
            Console.WriteLine($"Sharpe (KFC, annualisé) : {btKfcValue.ResultatPerformance.SharpeRatio:F2}");

            var btSbuxValue = new Backtester(strategieValue, donneesSbux) { RiskFreeRateAnnual = 0.02 };
            btSbuxValue.Executer();
            Console.WriteLine("\n[SBUX - Value]");
            btSbuxValue.AfficherResultats();
            Console.WriteLine($"Sharpe (SBUX, annualisé) : {btSbuxValue.ResultatPerformance.SharpeRatio:F2}");

            var btNflxValue = new Backtester(strategieValue, donneesNflx) { RiskFreeRateAnnual = 0.02 };
            btNflxValue.Executer();
            Console.WriteLine("\n[NFLX - Value]");
            btNflxValue.AfficherResultats();
            Console.WriteLine($"Sharpe (NFLX, annualisé) : {btNflxValue.ResultatPerformance.SharpeRatio:F2}");

            // ============================================
            // 6) BACKTESTS AVEC STRATÉGIE MOMENTUM
            // ============================================
            Console.WriteLine("\n\n=== BACKTESTS STRATÉGIE MOMENTUM ===\n");
            
            var btKfcMomentum = new Backtester(strategieMomentum, donneesKfc) { RiskFreeRateAnnual = 0.02 };
            btKfcMomentum.Executer();
            Console.WriteLine("[KFC - Momentum]");
            btKfcMomentum.AfficherResultats();
            Console.WriteLine($"Sharpe (KFC, annualisé) : {btKfcMomentum.ResultatPerformance.SharpeRatio:F2}");

            var btSbuxMomentum = new Backtester(strategieMomentum, donneesSbux) { RiskFreeRateAnnual = 0.02 };
            btSbuxMomentum.Executer();
            Console.WriteLine("\n[SBUX - Momentum]");
            btSbuxMomentum.AfficherResultats();
            Console.WriteLine($"Sharpe (SBUX, annualisé) : {btSbuxMomentum.ResultatPerformance.SharpeRatio:F2}");

            var btNflxMomentum = new Backtester(strategieMomentum, donneesNflx) { RiskFreeRateAnnual = 0.02 };
            btNflxMomentum.Executer();
            Console.WriteLine("\n[NFLX - Momentum]");
            btNflxMomentum.AfficherResultats();
            Console.WriteLine($"Sharpe (NFLX, annualisé) : {btNflxMomentum.ResultatPerformance.SharpeRatio:F2}");

            // ============================================
            // 7) Portefeuille KFC + SBUX + NFLX (Stratégie Value)
            // ============================================
            Console.WriteLine("\n\n=== PORTEFEUILLE (KFC + SBUX + NFLX) - STRATÉGIE VALUE ===\n");
            var pf = new PortefeuilleBacktester(strategieValue);
            File.Copy(kfcNorm, Path.Combine(dataDir, "KFC.csv"), overwrite: true);
            File.Copy(sbuxNorm, Path.Combine(dataDir, "SBUX.csv"), overwrite: true);
            File.Copy(nflxNorm, Path.Combine(dataDir, "NFLX.csv"), overwrite: true);
            pf.ChargerDonnees(dataDir, new List<string> { "KFC", "SBUX", "NFLX" });
            pf.Executer();
            pf.AfficherResultatsGlobaux();

            Console.WriteLine("\n✓ Backtest terminé.");
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
