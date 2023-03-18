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
        public async Task<IActionResult> Create(int number)
        {
            var saga = await _sagaManager.CreateAsync();
            // Create Order
            saga.AddStep(new CreateOrderEvent() { OrderName = "shoes", Number = number }, hasCompensation: true);
            // Deduct balance
            saga.AddStep(new DeductionEvent() { Money = 101 * number }, hasCompensation: true);
            // Reduce a pair of shoes in stock
            saga.AddStep(new ReduceStockEvent() { Number = number }, hasCompensation: true);

            await _sagaManager.StartAsync(saga);

            return Ok();
        }
    }
}
