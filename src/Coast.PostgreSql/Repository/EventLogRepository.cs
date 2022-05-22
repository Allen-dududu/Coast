namespace Coast.PostgreSql.Service
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;
    using Coast.Core.EventBus.EventLog;
    using Dapper;

    public class EventLogRepository : IEventLogRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly string _tableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogRepository"/> class.
        /// </summary>
        public EventLogRepository(string schemaName, IDbConnection connection, IDbTransaction transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
            _tableName = $"\"{schemaName}\".\"EventLog\"";
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

        public async Task<EventLogEntry> RetrieveEventLogsAsync(long eventId, CancellationToken cancellationToken = default)
        {
            string QueryEventLogSql =
$@"SELECT ""EventId"", ""CreationTime"", ""EventTypeName"", ""Content"", ""State"", ""TimesSent"" 
FROM {_tableName}  where ""EventId"" = @EventId;";

            return await _connection.QuerySingleOrDefaultAsync<EventLogEntry>(QueryEventLogSql, new { EventId = eventId }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<EventLogEntry>> RetrieveEventLogsPendingToPublishAsync(long eventId, CancellationToken cancellationToken = default)
        {
            string QueryEventLogSql =
$@"SELECT ""EventId"", ""CreationTime"", ""EventTypeName"", ""Content"", ""State"", ""TimesSent"" 
FROM {_tableName} where ""EventId"" = @EventId and ""State"" = {EventStateEnum.InProgress};";

            return await _connection.QueryAsync<EventLogEntry>(QueryEventLogSql, new { EventId = eventId }).ConfigureAwait(false);
        }

        public async Task SaveEventAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string InsertEventLogSql =
$@"INSERT INTO {_tableName} 
(""EventId"", ""CreationTime"", ""EventTypeName"", ""Content"", ""State"", ""TimesSent"") 
VALUES (@EventId, @CreationTime, @EventTypeName, @Content, @State, @TimesSent); ";

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
            foreach (var e in @events)
            {
                await SaveEventAsync(e, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateEventState(long eventId, EventStateEnum State, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string QueryEventLogSql =
$@"SELECT ""EventId"", ""CreationTime"", ""EventTypeName"", ""Content"", ""State"", ""TimesSent"" 
FROM {_tableName} where ""EventId"" = @EventId;";

            string UpdateEventLogSql =
$@"UPDATE {_tableName}
SET ""State"" = @State, ""TimesSent"" = @TimesSent 
WHERE ""EventId"" = @EventId";

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
