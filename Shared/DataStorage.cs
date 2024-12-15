using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace Shared
{
    public class DataStorage
    {
        private readonly string filePath;

        public DataStorage()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var directory = new DirectoryInfo(currentDirectory);

            while (directory != null && !directory.Name.Equals("CurrencyExchangeSystem", StringComparison.OrdinalIgnoreCase))
            {
                directory = directory.Parent;
            }

            if (directory == null)
            {
                throw new Exception("Solution root not found.");
            }

            var dataDirectory = Path.Combine(directory.FullName, "Shared", "Data");

            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            filePath = Path.Combine(dataDirectory, "exchange_rates.json");
        }


        public async Task SaveRatesAsync(IEnumerable<ExchangeRate> newRates)
        {
            var existingRates = await LoadRatesAsync();

            var updatedRates = existingRates
                .Where(existingRate => !newRates.Any(newRate => newRate.Pair == existingRate.Pair))
                .ToList();

            updatedRates.AddRange(newRates);

            var json = JsonConvert.SerializeObject(updatedRates, Newtonsoft.Json.Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<List<ExchangeRate>> LoadRatesAsync()
        {
            if (!File.Exists(filePath)) return new List<ExchangeRate>();
            var json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<List<ExchangeRate>>(json);
        }
    }
}