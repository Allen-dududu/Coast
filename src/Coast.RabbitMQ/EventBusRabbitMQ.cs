namespace Coast.RabbitMQ
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.DataLayer;
    using Coast.Core.EventBus;
    using Coast.Core.Util;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using global::RabbitMQ.Client.Exceptions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Retry;

    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        const string BROKER_NAME = "coast_event_bus";

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _retryCount;
        private readonly IProcessSagaEvent _processSagaEvent;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly string _callBackEventName;

        private IModel _consumerChannel;
        private string _queueName;

        public EventBusRabbitMQ(
            IRabbitMQPersistentConnection persistentConnection,
            ILogger<EventBusRabbitMQ> logger,
            IServiceProvider serviceProvider,
            IEventBusSubscriptionsManager subsManager,
            IProcessSagaEvent processSagaEvent,
            IRepositoryFactory repositoryFactory,
            CoastOptions coastOptions,
            string queueName = null,
            int retryCount = 5)
        {
            _serviceProvider = serviceProvider;
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _callBackEventName = coastOptions.DomainName + CoastConstant.CallBackEventSuffix;
            _queueName = queueName ?? coastOptions.DomainName;
            _consumerChannel = CreateConsumerChannel();
            _retryCount = retryCount;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
            _processSagaEvent = processSagaEvent;
            _repositoryFactory = repositoryFactory;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName,
                    exchange: BROKER_NAME,
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }

        public void Publish(IntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var eventName = @event.EventName ?? @event.GetType().Name;
            var message = JsonSerializer.Serialize(@event, @event.GetType());

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventName);

            Send(@event.Id, eventName, message);
        }

        public async Task PublishWithLogAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            using var session = _repositoryFactory.OpenSession();
            var eventLogRepository = session.ConstructEventLogRepository();
            if (null != await eventLogRepository.RetrieveEventLogsAsync(@event.Id).ConfigureAwait(false))
            {
                await eventLogRepository.MarkEventAsInProgressAsync(@event.Id).ConfigureAwait(false);
                Publish(@event);
                await eventLogRepository.MarkEventAsPublishedAsync(@event.Id).ConfigureAwait(false);
            }
            else
            {
                Publish(@event);
            }
        }

        private void Send(long eventId, string eventName, string message)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, "Could not publish event: {EventId}-{EventName} after {Timeout}s ({ExceptionMessage})", eventId, eventName, $"{time.TotalSeconds:n1}", ex.Message);
                });

            using (var channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}-{EventName}", eventId, eventName);

                channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");

                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", eventId);

                    channel.BasicPublish(
                        exchange: BROKER_NAME,
                        routingKey: eventName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);
                });
            }
        }

        public void Subscribe<T, TH>()
            where T : EventRequestBody
            where TH : ISagaHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).GetGenericTypeName());

            _subsManager.AddSubscription<T, TH>();
            StartBasicConsume();
        }

        public void Subscribe<TH>(string eventName) where TH : ISagaHandler
        {
            DoInternalSubscription(eventName);

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).GetGenericTypeName());

            _subsManager.AddSubscription<TH>(eventName);
            StartBasicConsume();
        }

        public void Unsubscribe<TH>(string eventName) where TH : ISagaHandler
        {
            _subsManager.RemoveSubscription<TH>(eventName);
        }

        public void Unsubscribe<T, TH>()
            where T : EventRequestBody
            where TH : ISagaHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _subsManager.RemoveSubscription<T, TH>();
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            _subsManager.Clear();
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                _consumerChannel.QueueBind(queue: _queueName,
                                    exchange: BROKER_NAME,
                                    routingKey: eventName);
            }
        }

        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (_consumerChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_Received;

                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var @event = JsonSerializer.Deserialize<SagaEvent>(message);

            // Call Back Event.
            if (string.Equals(eventName, _callBackEventName, StringComparison.Ordinal))
            {
                // process callback event.
                var callBackEventService = new CallBackEventService(_serviceProvider);
                var (reject, sagaEvents) = await callBackEventService.ProcessEventAsync(@event).ConfigureAwait(false);
                if (reject)
                {
                    _consumerChannel.BasicReject(eventArgs.DeliveryTag, requeue: true);
                }
                else
                {
                    _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                    if (sagaEvents != null)
                    {
                        foreach (var sagaEvent in sagaEvents)
                        {
                            await PublishWithLogAsync(sagaEvent).ConfigureAwait(false);
                        }
                    }
                }

                return;
            }

            // Saga Event
            try
            {
                await _processSagaEvent.IdempotentProcessEvent(eventName, @event).ConfigureAwait(false);
                @event.Succeeded = true;
            }
            catch (Exception ex)
            {
                @event.Succeeded = false;
                @event.ErrorMessage = ex.Message;
                @event.FailedCount++;
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);

                if (@event.FailedCount > 3)
                {
                    return;
                }

                // todo
                // 补偿消息失败到一定次数后，发送到死信队列，又用户决定处理。
            }

            // Even on exception we take the message off the queue.
            // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX).
            // For more information see: https://www.rabbitmq.com/dlx.html
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);

            var @callBackEvent = new SagaEvent()
            {
                StepId = @event.StepId,
                CorrelationId = @event.CorrelationId,
                Succeeded = @event.Succeeded,
                EventName = @event.CallBackEventName,
                ErrorMessage = @event.ErrorMessage,
            };

            await PublishWithLogAsync(@callBackEvent).ConfigureAwait(false);
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _logger.LogTrace("Creating RabbitMQ consumer channel");

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: BROKER_NAME,
                                    type: "direct");

            channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

                _consumerChannel?.Dispose();
                _consumerChannel = CreateConsumerChannel();
                StartBasicConsume();
            };

            return channel;
        }
    }
}
