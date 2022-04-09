namespace Coast.Core.EventBus.IntegrationEventLogEF
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore.Storage;

    public interface IIntegrationEventLogService
    {
        Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);

        /// <summary>
        /// save the event.
        /// </summary>
        /// <param name="event">event.</param>
        /// <param name="transaction"> A transaction against the database.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);

        Task MarkEventAsPublishedAsync(Guid eventId);

        Task MarkEventAsInProgressAsync(Guid eventId);

        Task MarkEventAsFailedAsync(Guid eventId);
    }
}
