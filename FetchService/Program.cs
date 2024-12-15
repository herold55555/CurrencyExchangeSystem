using CurrencyExchangeSystem.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var currencyPairs = new List<string>
        {
            "EUR/USD",
            "EUR/JPY",
            "GBP/EUR",
            "USD/ILS"
        };

        var fetchService = new FetchExchangeRates(currencyPairs);
        await fetchService.StartAsync();
    }
}
