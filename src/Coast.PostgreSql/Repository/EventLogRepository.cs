namespace Coast.PostgreSql.Service
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.EventBus;
    using Coast.Core.EventBus.IntegrationEventLog;
    using Coast.PostgreSql.Connection;
    using Dapper;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Options;

    public class EventLogRepository : IEventLogRepository
    {
        private const string InsertEventLogSql =
@"INSERT INTO ""Coast_EventLog"" 
(""EventId"", ""CreationTime"", ""EventTypeName"", ""Content"", ""State"", ""TimesSent"") 
VALUES (@EventId, @CreationTime, @EventTypeName, @Content, @State, @TimesSent); ";

        private const string QueryEventLogSql =
@"SELECT ""EventId"", ""CreationTime"", ""EventTypeName"", ""Content"", ""State"", ""TimesSent"" 
FROM ""Coast_EventLog"" where ""EventId"" = @EventId;";

        private const string UpdateEventLogSql =
@"UPDATE ""Coast_EventLog""
SET ""State"" = @State, ""TimesSent"" = @TimesSent 
WHERE ""EventId"" = @EventId";

        private IDbConnection _connection;
        private IDbTransaction _transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogRepository"/> class.
        /// </summary>
        public EventLogRepository(IDbConnection connection, IDbTransaction transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public Task MarkEventAsFailedAsync(long eventId, CancellationToken cancellationToken = default)
        {
            return UpdateEventState(eventId, EventStateEnum.PublishedFailed);
        }

        public Task MarkEventAsInProgressAsync(long eventId, CancellationToken cancellationToken = default)
        {
            return UpdateEventState(eventId, EventStateEnum.InProgress);
        }

        public Task MarkEventAsPublishedAsync(long eventId, CancellationToken cancellationToken = default)
        {
            return UpdateEventState(eventId, EventStateEnum.Published);
        }

        public Task<IEnumerable<EventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task SaveEventAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var eventLogEntry = new EventLogEntry(@event);

            await _connection.ExecuteAsync(
                    InsertEventLogSql,
                    new
                    {
                        EventId = eventLogEntry.EventId,
                        CreationTime = eventLogEntry.CreationTime,
                        EventTypeName = eventLogEntry.EventTypeName,
                        Content = eventLogEntry.Content,
                        State = eventLogEntry.State,
                        TimesSent = eventLogEntry.TimesSent
                    },
                    transaction: _transaction).ConfigureAwait(false);
        }

        public async Task SaveEventAsync(IEnumerable<IntegrationEvent> @events, CancellationToken cancellationToken = default)
        {
            foreach(var e in @events)
            {
                await SaveEventAsync(e, cancellationToken);
            }
        }

        private async Task UpdateEventState(long eventId, EventStateEnum State)
        {
            var eventLogEntry = await _connection.QuerySingleAsync<EventLogEntry>(
                QueryEventLogSql,
                new
                {
                    EventId = eventId
                }).ConfigureAwait(false);

            eventLogEntry.State = State;

            if (State == EventStateEnum.InProgress)
            {
                eventLogEntry.TimesSent++;
            }

            await _connection.ExecuteAsync(
                UpdateEventLogSql,
                new
                {
                    EventId = eventId,
                    State = eventLogEntry.State,
                    TimesSent = eventLogEntry.TimesSent
                }).ConfigureAwait(false);
        }
    }
}
