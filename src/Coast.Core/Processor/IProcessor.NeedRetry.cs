namespace Coast.Core.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;
    using Coast.Core.EventBus.EventLog;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class MessageNeedToRetryProcessor : IProcessor
    {
        private readonly TimeSpan _delay = TimeSpan.FromSeconds(1);
        private readonly ILogger<MessageNeedToRetryProcessor> _logger;
        private readonly IEventBus _messageSender;
        private readonly TimeSpan _waitingInterval;

        public MessageNeedToRetryProcessor(
            ILogger<MessageNeedToRetryProcessor> logger,
            IEventBus messageSender)
        {
            _logger = logger;
            _messageSender = messageSender;
            _waitingInterval = TimeSpan.FromMinutes(10);
        }

        public async Task ProcessAsync(ProcessingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            using var unitofWork = context.Provider.GetRequiredService<IUnitOfWork>();
            await ProcessPublishedAsync(unitofWork.EventLogRepository, context);
            unitofWork.Commit();
            await context.WaitAsync(_waitingInterval);
        }

        private async Task ProcessPublishedAsync(IEventLogRepository eventLog, ProcessingContext context)
        {
            context.ThrowIfStopping();

            var messages = await GetSafelyAsync<EventLogEntry>(() => eventLog.RetrieveEventLogsPendingToPublishAsync());

            foreach (var message in messages)
            {
                _messageSender.Publish(message.EventId, eventName: message.EventTypeName, message: message.Content);
                await eventLog.MarkEventAsPublishedAsync(message.EventId);
                await context.WaitAsync(_delay);
            }
        }

        private async Task<IEnumerable<T>> GetSafelyAsync<T>(Func<Task<IEnumerable<T>>> getMessagesAsync)
        {
            try
            {
                return await getMessagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(1, ex, "Get messages from storage failed. Retrying...");

                return Enumerable.Empty<T>();
            }
        }
    }
}
