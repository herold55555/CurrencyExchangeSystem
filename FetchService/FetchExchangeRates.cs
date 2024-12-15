using CurrencyExchangeSystem.Models;
using CurrencyExchangeSystem.Services;
using HtmlAgilityPack;
using Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CurrencyExchangeSystem.Services
{
    public class FetchExchangeRates
    {
        private readonly List<string> currencyPairs;
        private readonly BufferBlock<List<ExchangeRate>> buffer = new BufferBlock<List<ExchangeRate>>();
        private readonly DataStorage dataStorage = new DataStorage();
        private readonly int fetchIntervalSeconds = 10;
        private readonly string url = "https://www.xe.com/currencyconverter/";

        public FetchExchangeRates(List<string> pairs)
        {
            currencyPairs = pairs;
        }

        public async Task StartAsync()
        {
            var producerTask = Task.Run(() => ProduceAsync());
            var consumerTask = Task.Run(() => ConsumeAsync());

            await Task.WhenAll(producerTask, consumerTask);
        }

        private async Task ProduceAsync()
        {
            while (true)
            {
                try
                {
                    var rates = new List<ExchangeRate>();

                    foreach (var pair in currencyPairs)
                    {
                        var rate = await FetchRateAsync(pair);
                        if (rate != null)
                        {
                            rates.Add(rate);
                        }
                    }

                    if (rates.Count > 0)
                    {
                        await buffer.SendAsync(rates);
                        Console.WriteLine($"{DateTime.Now}: Produced rates and added to buffer.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in producer: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(fetchIntervalSeconds));
            }
        }

        public async Task<ExchangeRate> FetchRateAsync(string pair)
        {
            try
            {
                var fromCurrency = pair.Split('/')[0];
                var toCurrency = pair.Split('/')[1];
                var fullUrl = $"https://www.xe.com/currencyconverter/convert/?Amount=1&From={fromCurrency}&To={toCurrency}";

                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(fullUrl);

                var innerText = doc.DocumentNode.ChildNodes[1].InnerText;

                var searchString = "1.00";
                var startIndex = innerText.IndexOf(searchString, StringComparison.OrdinalIgnoreCase);

                if (startIndex != -1)
                {
                    var substring = innerText.Substring(startIndex, 50);

                    var parts = substring.Split('=');
                    if (parts.Length > 1)
                    {
                        var rateString = parts[1].Split(' ')[0].Trim();

                        if (double.TryParse(rateString, NumberStyles.Any, CultureInfo.InvariantCulture, out double rateValue))
                        {
                            return new ExchangeRate
                            {
                                Pair = pair,
                                Value = rateValue,
                                LastUpdated = DateTime.Now
                            };
                        }
                        else
                        {
                            Console.WriteLine($"Could not parse rate for {pair}. Rate text: {rateString}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Search string '1.00' not found for {pair}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching rate for {pair}: {ex.Message}");
                return null;
            }
        }

        private async Task ConsumeAsync()
        {
            while (true)
            {
                try
                {
                    var rates = await buffer.ReceiveAsync();

                    await dataStorage.SaveRatesAsync(rates);

                    Console.WriteLine($"{DateTime.Now}: Consumed rates and saved to file.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in consumer: {ex.Message}");
                }
            }
        }
    }
}
