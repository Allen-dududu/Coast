namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;
    using Coast.Core.EventBus.EventLog;

    public interface IEventLogRepository
    {
        Task<IEnumerable<EventLogEntry>> RetrieveEventLogsPendingToPublishAsync();

        Task<EventLogEntry> RetrieveEventLogsAsync(long eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// save the event.
        /// </summary>
        /// <param name="event">event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SaveEventAsync(IntegrationEvent @event, CancellationToken cancellationToken = default);

        Task SaveEventAsync(IEnumerable<IntegrationEvent> @events, CancellationToken cancellationToken = default);

        Task MarkEventAsPublishedAsync(long eventId, CancellationToken cancellationToken = default);

        Task MarkEventAsInProgressAsync(long eventId, CancellationToken cancellationToken = default);

        Task MarkEventAsFailedAsync(long eventId, CancellationToken cancellationToken = default);
    }
}
