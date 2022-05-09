namespace Coast.Core.EventBus.EventLog
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore.Storage;

    public interface IEventLogRepository
    {
        Task<IEnumerable<EventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId, CancellationToken cancellationToken = default);

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
