using Coast.Core.EventBus;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static Coast.Core.EventBus.InMemoryEventBusSubscriptionsManager;

namespace Coast.Core.Test
{
    public class ProcessSagaEventTest
    {
        private readonly ProcessSagaEvent _sut;
        private readonly Mock<ILogger<ProcessSagaEvent>> _logger = new Mock<ILogger<ProcessSagaEvent>>();
        private readonly Mock<IEventBusSubscriptionsManager> _subsManager = new Mock<IEventBusSubscriptionsManager>();
        private readonly Mock<IServiceProvider> _serviceProvider = new Mock<IServiceProvider>();
        private readonly Mock<IBarrierService> _barrierService = new Mock<IBarrierService>();
        public ProcessSagaEventTest()
        {
            _sut = new ProcessSagaEvent(_serviceProvider.Object, _logger.Object, _subsManager.Object, _barrierService.Object);
        }

        [Fact]
        public async Task ProcessSagaEvent_InvokeCommitMethod_NotDynamicAsync()
        {
            // Arranage
            var eventName = "testCommit";
            var sagaEvent = new SagaEvent();
            sagaEvent.EventName = eventName;
            sagaEvent.StepType = TransactionStepTypeEnum.Commit;
            sagaEvent.RequestBody = JsonConvert.SerializeObject(new SagaMessageMock() { Money = 101 });
            //var message = "{\"CorrelationId\":5035038954235457537,\"SagaStepId\":5035038954235457538,\"EventType\":0,\"RequestBody\":\"{\\\"Money\\\":101}\",\"ErrorMessage\":null,\"Headers\":null,\"Id\":5035038954235457540,\"CreationDate\":\"2022-05-08T17:16:41.7827586Z\",\"EventName\":\"DeductionRequest\",\"TransactionType\":0,\"Succeeded\":false,\"DomainName\":\"OrderManagement\"}";
            var message = JsonConvert.SerializeObject(sagaEvent);
            var sagaHandlerMock = new SagaHandlerMock();

            _subsManager.Setup(x => x.HasSubscriptionsForEvent(eventName)).Returns(true);
            _subsManager.Setup(x => x.GetEventTypeByName(eventName)).Returns(typeof(SagaMessageMock));
            _subsManager.Setup(x => x.GetHandlersForEvent(eventName)).Returns(new List<SubscriptionInfo>() { SubscriptionInfo.Typed(typeof(SagaHandlerMock)) });

            _serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(sagaHandlerMock);

            // Act
            await _sut.ProcessEvent(eventName, sagaEvent);

            // Assert
            Assert.Equal("Commit", sagaHandlerMock.ExecutedMethod);
        }

        [Fact]
        public async Task ProcessSagaEvent_InvokeCancelMethod_NotDynamicAsync()
        {
            // Arranage
            var eventName = "testCommit";
            var sagaEvent = new SagaEvent();
            sagaEvent.EventName = eventName;
            sagaEvent.StepType = TransactionStepTypeEnum.Compensate;
            sagaEvent.RequestBody = JsonConvert.SerializeObject(new SagaMessageMock() { Money = 101 });

            var sagaHandlerMock = new SagaHandlerMock();

            _subsManager.Setup(x => x.HasSubscriptionsForEvent(eventName)).Returns(true);
            _subsManager.Setup(x => x.GetEventTypeByName(eventName)).Returns(typeof(SagaMessageMock));
            _subsManager.Setup(x => x.GetHandlersForEvent(eventName)).Returns(new List<SubscriptionInfo>() { SubscriptionInfo.Typed(typeof(SagaHandlerMock)) });

            _serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(sagaHandlerMock);

            // Act
            await _sut.ProcessEvent(eventName, sagaEvent);

            // Assert
            Assert.Equal("Cancel", sagaHandlerMock.ExecutedMethod);
        }

        [Fact]
        public async Task ProcessSagaEvent_InvokeCommitMethod_DynamicAsync()
        {
            // Arranage
            var eventName = "testCommit";
            var sagaEvent = new SagaEvent();
            sagaEvent.EventName = eventName;
            sagaEvent.StepType = TransactionStepTypeEnum.Commit;
            sagaEvent.RequestBody = JsonConvert.SerializeObject(new SagaMessageMock() { Money = 101 });

            var sagaHandlerMock = new SagaHandlerMock2();

            _subsManager.Setup(x => x.HasSubscriptionsForEvent(eventName)).Returns(true);
            _subsManager.Setup(x => x.GetEventTypeByName(eventName)).Returns(typeof(SagaMessageMock));
            _subsManager.Setup(x => x.GetHandlersForEvent(eventName)).Returns(new List<SubscriptionInfo>() { SubscriptionInfo.Dynamic(typeof(SagaHandlerMock)) });

            _serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(sagaHandlerMock);

            // Act
            await _sut.ProcessEvent(eventName, sagaEvent);

            // Assert
            Assert.Equal("Commit", sagaHandlerMock.ExecutedMethod);
        }

        [Fact]
        public async Task ProcessSagaEvent_InvokeCancelMethod_DynamicAsync()
        {
            // Arranage
            var eventName = "testCommit";
            var sagaEvent = new SagaEvent();
            sagaEvent.EventName = eventName;
            sagaEvent.StepType = TransactionStepTypeEnum.Compensate;
            sagaEvent.RequestBody = JsonConvert.SerializeObject(new SagaMessageMock() { Money = 101 });

            var sagaHandlerMock = new SagaHandlerMock2();

            _subsManager.Setup(x => x.HasSubscriptionsForEvent(eventName)).Returns(true);
            _subsManager.Setup(x => x.GetEventTypeByName(eventName)).Returns(typeof(SagaMessageMock));
            _subsManager.Setup(x => x.GetHandlersForEvent(eventName)).Returns(new List<SubscriptionInfo>() { SubscriptionInfo.Dynamic(typeof(SagaHandlerMock)) });

            _serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(sagaHandlerMock);

            // Act
            await _sut.ProcessEvent(eventName, sagaEvent);

            // Assert
            Assert.Equal("Cancel", sagaHandlerMock.ExecutedMethod);
        }

        public class SagaMessageMock : EventRequestBody
        {
            public long Money { get; set; }
        }

        public class SagaHandlerMock : ISagaHandler<SagaMessageMock>
        {
            public string ExecutedMethod { get; set; }
            public SagaHandlerMock()
            {

            }
            public async Task CancelAsync(SagaMessageMock @event)
            {
                await Task.Yield();
                ExecutedMethod = "Cancel";
            }

            public async Task CommitAsync(SagaMessageMock @event)
            {
                await Task.Yield();
                ExecutedMethod = "Commit";
            }
        }

        public class SagaHandlerMock2 : ISagaHandler
        {
            public string ExecutedMethod { get; set; }
            public async Task CancelAsync(string @event)
            {
                await Task.Yield();
                ExecutedMethod = "Cancel";
            }

            public async Task CommitAsync(string @event)
            {
                await Task.Yield();
                ExecutedMethod = "Commit";
            }
        }

    }
}
