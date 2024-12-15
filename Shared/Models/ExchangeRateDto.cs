using System;

namespace CurrencyExchangeSystem.Models
{
    public class ExchangeRateDto
    {
        public string CurrencyPair { get; set; }
        public double Rate { get; set; }
        public string LastUpdated { get; set; }
    }
}