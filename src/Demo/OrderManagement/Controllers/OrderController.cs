using Coast.Core;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.SagaEvents.Events;

namespace OrderManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ISagaManager _sagaManager;

        public OrderController(ILogger<OrderController> logger, ISagaManager sagaManager)
        {
            _sagaManager = sagaManager;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create()
        {
            var saga = await _sagaManager.CreateAsync();

            // Deduct $100
            saga.AddStep(new DeductionRequest() { Money = 101 }, hasCompensation: true, executeOrder: 1);
            // Reduce a pair of shoes in stock
            saga.AddStep(new ReduceStockRequest() { Number = 1 }, hasCompensation: true, executeOrder: 1);

            await _sagaManager.StartAsync(saga);

            return Ok();
        }
    }
}
