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
            // Create Order
            saga.AddStep(new CreateOrderEvent() { OrderName = "Buy a pair of shoes" }, hasCompensation: true);
            // Deduct $100
            saga.AddStep(new DeductionEvent() { Money = 101 }, hasCompensation: true);
            // Reduce a pair of shoes in stock
            saga.AddStep(new ReduceStockEvent() { Number = 1 }, hasCompensation: true);
            await _sagaManager.StartAsync(saga);
            return Ok();
        }
    }
}
