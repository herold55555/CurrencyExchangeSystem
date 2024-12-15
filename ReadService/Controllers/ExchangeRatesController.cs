using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using CurrencyExchangeSystem.Models;

namespace ReadService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly DataStorage _dataStorage = new DataStorage();

        [HttpGet]
        public async Task<IActionResult> GetExchangeRates()
        {
            var rates = await _dataStorage.LoadRatesAsync();

            if (rates == null || rates.Count == 0)
            {
                return NotFound(new
                {
                    Status = "Error",
                    Message = "No exchange rate data available.",
                    Timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            var ratesDto = rates.Select(rate => new ExchangeRateDto
            {
                CurrencyPair = rate.Pair,
                Rate = rate.Value,
                LastUpdated = rate.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return Ok(ratesDto);
        }
    }
}
