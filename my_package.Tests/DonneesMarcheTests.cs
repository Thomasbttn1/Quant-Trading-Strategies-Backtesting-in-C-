using System;
using System.IO;
using QuantBacktest;
using Xunit;

namespace my_package.Tests
{
    public class DonneesMarcheTests
    {
        [Fact]
        public void ChargerDepuisCSV_Reads_valid_rows()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Date,Open,High,Low,Close,Volume\n2022-01-01,1,2,0.5,1.5,123\n2022-01-02,1.5,2.1,1.0,2.0,456");

            var d = new DonneesMarche();
            d.ChargerDepuisCSV(tmp);

            Assert.Equal(2, d.Cotations.Count);
            Assert.Equal(new DateTime(2022,1,1), d.Cotations[0].Date);
            Assert.Equal(1, d.Cotations[0].Open);
            Assert.Equal(2.1, d.Cotations[1].High);
        }

        [Fact]
        public void ToString_Returns_summary()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "Date,Open,High,Low,Close,Volume\n2022-01-01,1,2,0.5,1.5,123\n2022-01-02,1.5,2.1,1.0,2.0,456");

            var d = new DonneesMarche();
            d.ChargerDepuisCSV(tmp);
            var s = d.ToString();
            Assert.Contains("Données de marché", s);
            Assert.Contains("2022-01-01", s);
            Assert.Contains("2022-01-02", s);
        }
    }
}
