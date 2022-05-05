using Coast.Core;
using Microsoft.AspNetCore.Mvc;

namespace Saga_RabbitMQ_PostgreSql.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ISagaManager _sagaManager;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ISagaManager sagaManager)
        {
            _logger = logger;
            _sagaManager = sagaManager;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("tradeIn")]
        public async Task<IActionResult> Get2Async()
        {
            var saga = await _sagaManager.CreateAsync();
            saga.AddStep(new TradeInRequest() { Amount = 100 });

            await _sagaManager.StartAsync(saga);

            return Ok();
        }
    }
}