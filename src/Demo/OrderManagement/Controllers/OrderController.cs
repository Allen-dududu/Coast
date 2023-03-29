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

        [HttpPost("Saga")]
        public async Task<IActionResult> Create(int number)
        {
            var saga = await _sagaManager.CreateAsync();
            // Create Order
            saga.AddStep(new CreateOrderEvent() { OrderName = "shoes", Number = number });
            // Deduct balance
            saga.AddStep(new DeductionEvent() { Money = 101 * number });
            // Reduce a pair of shoes in stock
            saga.AddStep(new ReduceStockEvent() { Number = number });

            await _sagaManager.StartAsync(saga);

            return Ok();
        }

        [HttpPost("TCC")]
        public async Task<IActionResult> TCC(int number)
        {
            var saga = await _sagaManager.CreateAsync();

            // Try, Cancel
            saga.AddStep(new CreateOrderEvent() { OrderName = "shoes", Number = number }, hasCompensation: true, executionSequenceNumber: 1);
            saga.AddStep(new DeductionEvent() { Money = 101 * number }, hasCompensation: true, executionSequenceNumber: 1);
            saga.AddStep(new ReduceStockEvent() { Number = number }, hasCompensation: true, executionSequenceNumber: 1);

            // Commit
            saga.AddStep(new CreateOrderCommitEvent() { OrderName = "shoes", Number = number }, hasCompensation: false, executionSequenceNumber: 2);
            saga.AddStep(new DeductionCommitEvent() { Money = 101 * number }, hasCompensation: false, executionSequenceNumber: 2);
            saga.AddStep(new ReduceStockCommitEvent() { Number = number }, hasCompensation: false, executionSequenceNumber: 2);

            await _sagaManager.StartAsync(saga);

            return Ok();
        }
    }
}
