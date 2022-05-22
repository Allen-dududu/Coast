# Coast 　　　　　　　　　　　　　　　　　　　　                      [中文](https://github.com/Allen-dududu/Coast/blob/main/README.zh-cn.md)

[![Build And Automated Tests](https://github.com/Allen-dududu/Coast/actions/workflows/build.yaml/badge.svg)](https://github.com/Allen-dududu/Coast/actions/workflows/build.yaml)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Coast.Core)](https://nuget.org/packages/Coast.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Coast is a decentralized message broker-based compensation distributed transaction framework. The biggest difference from other distributed transaction frameworks (Seata, DTM...) is that there is no need to install additional global transaction management nodes.

Distributed transaction frameworks usually guarantee eventual consistency by rolling back transactions. If one or more steps fail, undo the work performed by a sequence of steps that together define an eventually consistent operation. For example TCC and Saga transaction model.

## Saga
The Saga model is to split a distributed transaction into multiple local transactions. Each local transaction has a corresponding execution module and compensation module. When any local transaction in the Saga transaction fails, it can be restored by calling the relevant compensation method. to achieve eventual transaction consistency.

### Nuget

You can run the following command to install Coast in your project.

```
PM> Install-Package Coast.Core
```

Coast's current database supports PostgreSQL. The message broker supports RabbitMQ. But more will be supported in the future, and PRs are welcome.
```
PM> Install-Package Coast.PostgreSql
PM> Install-Package Coast.RabbitMQ
```

### Configuration
Configuration in .net6:

```c#
using Coast.PostgreSql;
using Coast.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCosat(x =>
{
    x.DomainName = "The name of this service";
    x.UseRabbitMQ("ConnectionStrings");
    x.UsePostgreSql("ConnectionStrings");
});

```

### Create Saga

For example, an online shopping mall website, we have three services, OrderManagement, Payment, Stock.
To purchase a product, a customer needs to create an order in the order service, deduct the payment in the payment service, and reduce the inventory of the product in the Stock service.

First, let's create a Saga in the controller of the order service.

```c#
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ISagaManager _sagaManager;

        // Inject ISagaManager service
        public OrderController(ISagaManager sagaManager)
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

            // Coast will send events in the order in which the saga steps were added (provided that the previous event was successfully processed).
            await _sagaManager.StartAsync(saga);
        }
    }
```
The event object needs to inherit the Coast.Core.EventRequestBody class.

### Handle Saga Event

Recommended directory structure
```
├─ SagaEvents
   ├─ EventHandling // put CreateOrderEventHandler, DeductionEventHandler...
   ├─ Events //put CreateOrderEvent，DeductionEvent...
```

OrderManagement service handles the CreateOrderEvent event：
```c#
    public class CreateOrderEventHandler : ISagaHandler<CreateOrderEvent>
    {
        public Task CancelAsync(CreateOrderEvent @event)
        {
            // business code, cancel order
        }

        public Task CommitAsync(CreateOrderEvent @event)
        {
            // business code, create order
        }
    }
```
Payment service handles the DeductionEvent event：
```c#
    public class DeductionEventHandler : ISagaHandler<DeductionEvent>
    {
        public Task CancelAsync(DeductionEvent @event)
        {
            // Recharge the account.
        }

        public Task CommitAsync(DeductionEvent @event)
        {
            // Debit account
        }
    }
```

Stockservice handles the ReduceStockEvent event：
```c#
    public class ReduceStockEventHandler : ISagaHandler<ReduceStockEvent>
    {
        public Task CancelAsync(ReduceStockEvent @event)
        {
            // Inventory plus 1
            // Generally speaking, the last step of Saga could not provide compensation operation, could not implement CancelAsync method
            // In addition, if a previous step does not has compensation, hasCompensation should set to false
        }

        public Task CommitAsync(ReduceStockEvent @event)
        {
            // Inventory minus 1.
        }
    }
```

#### All event handling classes need to be registered in DI and Coast
```c#
// DI
builder.Services.AddTransient<DeductionEventHandler>();

app.CoastSubscribe<DeductionRequest, DeductionEventHandler>();
```

### Saga step sequential control and concurrencys
When Coast creates a Saga step, it provides the executionSequenceNumber parameter to control the sequence and concurrency of the steps.
```c#
saga.AddStep(new CreateOrderEvent() { OrderName = "Buy a pair of shoes" }, hasCompensation: true, executionSequenceNumber: 1);
saga.AddStep(new DeductionEvent() { Money = 101 }, hasCompensation: true, executionSequenceNumber: 2);
saga.AddStep(new ReduceStockEvent() { Number = 1 }, hasCompensation: true, executionSequenceNumber: 3);
// The order of execution is: CreateOrderEvent -> DeductionEvent -> ReduceStockEvent
```

Concurrency. When some steps can operate together, the executionSequenceNumber is set to the same number.
Note that when the executionSequenceNumber is set to the same step, if one of them fails, the Cancel method will not be executed immediately. the compensation step will be executed after the other executionSequenceNumber of the same step is executed.

### Idempotency
Coast provides default idempotent judgment, so users do not need to write judgment logic.

## TCC

TCC is a kind of compensation transaction, which requires each service to provide Try, Confirm and Cancel interfaces. Its core idea is to release the lock of resources as soon as possible by reserving resources. If the transaction can be committed, the confirmation of reserved resources will be completed.  

Next Release ---------------