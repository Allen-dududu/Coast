# Coast

[![Build And Automated Tests](https://github.com/Allen-dududu/Coast/actions/workflows/build.yaml/badge.svg)](https://github.com/Allen-dududu/Coast/actions/workflows/build.yaml)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Coast.Core)](https://nuget.org/packages/Coast.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Coast是一个去中心化基于消息代理的补偿分布式事务框架。与其他分布式事务框架（Seata,DTM...）最大的区别在于不需要安装额外的全局事务管理节点。

分布式事务框架通常以回滚事务的方式来保证最终一致性。如果一个或多个步骤失败，撤销由一系列步骤执行的工作，这些步骤一起定义了一个最终一致的操作。例如TCC和Saga事务模型。

## Saga
Saga模型是把一个分布式事务拆分为多个本地事务，每个本地事务都有相应的执行模块和补偿模块，当Saga事务中任意一个本地事务出错时，可以通过调用相关的补偿方法恢复之前的事务，达到事务最终一致性。

## TCC
TCC 也是一种补偿型事务，该模型要求应用的每个服务提供 Try、Confirm、Cancel 三个接口，它的核心思想是通过对资源的预留，尽早释放对资源的加锁，如果事务可以提交，则完成对预留资源的确认，如果事务要回滚，则释放预留的资源。。

### Nuget

你可以运行以下命令在你的项目中安装Coast。

```
PM> Install-Package Coast.Core
```

Coast 目前数据库支持PostgreSQL。消息代理支持RabbitMQ。但是以后会支持更多，同时欢迎提交PR。
```
PM> Install-Package Coast.PostgreSql
PM> Install-Package Coast.RabbitMQ
```

### Configuration
在.net6中的配置：

```c#
using Coast.PostgreSql;
using Coast.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCosat(x =>
{
    x.DomainName = "这个服务的名称，如付款服务，可以叫Payment";
    x.UseRabbitMQ("ConnectionStrings");
    x.UsePostgreSql("数据库连接字符串");
});

```

### 创建Saga

比如说一个在线商城网站，我们有三个服务，分别为订单服务（OrderManagement），付款服务（Payment），库存服务（Stock）。
客户购买一个商品需要在订单服务里创建订单，在付款服务里进行扣款，库存服务里对商品库存进行减少。

首先我们在订单服务的controller里我们来创建一个Saga。
```c#
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ISagaManager _sagaManager;

        // 注入ISagaManager服务
        public OrderController(ISagaManager sagaManager)
        {
            _sagaManager = sagaManager;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create()
        {
            var saga = await _sagaManager.CreateAsync();
            // Create Order
            saga.AddStep(new CreateOrderEvent() { OrderName = "Buy a pair of shoes" });
            // Deduct $100
            saga.AddStep(new DeductionEvent() { Money = 101 });
            // Reduce a pair of shoes in stock
            saga.AddStep(new ReduceStockEvent() { Number = 1 });

            // Coast会按照添加saga步骤的顺序，依次发送事件（前提是上个事件被成功处理）。
            await _sagaManager.StartAsync(saga);
        }
    }
```
事件对象需要继承Coast.Core.EventRequestBody类。

### 处理Saga事件

推荐的目录结构
```
├─ SagaEvents
   ├─ EventHandling 
   ├─ Events // saga事件类目录。CreateOrderEvent，DeductionEvent...
```

OrderManagement服务的Saga事件处理类处理CreateOrderEvent事件：
```c#
    public class CreateOrderEventHandler : ISagaHandler<CreateOrderEvent>
    {
        public Task CancelAsync(CreateOrderEvent @event)
        {
            // 业务代码，取消订单
        }

        public Task CommitAsync(CreateOrderEvent @event)
        {
            // 业务代码，创建订单
        }
    }
```

Payment服务的Saga事件处理类处理DeductionEvent事件：
```c#
    public class DeductionEventHandler : ISagaHandler<DeductionEvent>
    {
        public Task CancelAsync(DeductionEvent @event)
        {
            // 对账号进行充值。
        }

        public Task CommitAsync(DeductionEvent @event)
        {
            // 对账号进行扣款
        }
    }
```

Stock服务的Saga事件处理类处理ReduceStockEvent事件：
```c#
    public class ReduceStockEventHandler : ISagaHandler<ReduceStockEvent>
    {
        public Task CancelAsync(ReduceStockEvent @event)
        {
            // 库存加1
            // 一般来说Saga的最后一步可以不用提供补偿操作，可以不用实现CancelAsync方法
            // 另外，如果之前的某个步骤不需要补偿，hasCompensation设置为false
        }

        public Task CommitAsync(ReduceStockEvent @event)
        {
            // 库存减1
        }
    }
```

#### 所有的事件处理类都需要在DI和Coast里进行注册
```c#
// DI
builder.Services.AddTransient<DeductionEventHandler>();

// Saga事件处理类关联Saga步骤事件。
app.CoastSubscribe<DeductionRequest, DeductionEventHandler>();
```

### Saga步骤顺序控制与并发
Coast在创建Saga步骤时，提供executionSequenceNumber参数来控制步骤是顺序和并发。
```c#
saga.AddStep(new CreateOrderEvent() { OrderName = "Buy a pair of shoes" }, hasCompensation: true, executionSequenceNumber: 1);
saga.AddStep(new DeductionEvent() { Money = 101 }, hasCompensation: true, executionSequenceNumber: 2);
saga.AddStep(new ReduceStockEvent() { Number = 1 }, hasCompensation: true, executionSequenceNumber: 3);
// 执行顺序为: CreateOrderEvent -> DeductionEvent -> ReduceStockEvent
```

并发。当一些步骤可以在一起操作时，executionSequenceNumber设置为相同的数字。
注意，executionSequenceNumber设置相同的步骤，如果其中有一个失败了，会等其他executionSequenceNumber相同步骤执行完后，再执行补偿步骤，不会立刻执行。

### 幂等性
Coast提供了默认的幂等判断，所以不需要用户编写判断逻辑。

## TCC

Coast虽然没有提供TCC操作的接口，但是你依旧可以利用Saga步骤顺序控制来实现：
```c#
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
```
上面有两组并发Saga，第一组承担着TCC中的Try，Cancel操作，在Try中不再是直接扣款，或者直接减少库存，而且选择冻结资源。如果冻结资源时出现错误，则直接执行Cancel操作，如果成功，则进入第二组，第二组没有补偿操作，并且不容许失败，负责TCC中的Commit操作，解除冻结的资源，进行扣款或者是减少库存。

