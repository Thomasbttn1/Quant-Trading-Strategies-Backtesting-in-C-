using System;
namespace QuantBacktest
{
    public class SignalTrading
    {
        public DateTime Date { get; set; }
        public TypeSignal Type { get; set; } // "BUY" or "SELL"
        public double Price { get; set; }
    // Alias en français pour compatibilité avec le reste du code
        public double Prix { get => Price; set => Price = value; }
        public double Quantity { get; set; } = 0;


        public SignalTrading(DateTime date, TypeSignal type, double price)
        {
            Date = date;
            Type = type;
            Price = price;
        }

        public override string ToString()
        {
            return $"{Date.ToShortDateString()} - {Type} {Quantity} at {Price}";
        }

        public enum TypeSignal
        {
            BUY,
            SELL
        }
    }
}