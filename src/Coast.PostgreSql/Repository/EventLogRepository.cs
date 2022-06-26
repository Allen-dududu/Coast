namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.EventBus;
    using Coast.Core.EventBus.EventLog;
    using Dapper;

    internal class EventLogRepository : RepositoryBase, IEventLogRepository
    {
        private readonly string _tableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogRepository"/> class.
        /// </summary>
        public EventLogRepository(string schemaName, IDbTransaction transaction) : base(transaction)
        {
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

            return await Connection.QuerySingleOrDefaultAsync<EventLogEntry>(QueryEventLogSql, new { EventId = eventId }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<EventLogEntry>> RetrieveEventLogsPendingToPublishAsync()
        {
            string QueryEventLogSql =
$@"SELECT ""EventId"", ""CreationTime"", ""EventTypeName"", ""Content"", ""State"", ""TimesSent"" 
FROM {_tableName} where  ""State"" = @InProgress or ""State"" = @NotPublished;";

            return await Connection.QueryAsync<EventLogEntry>(QueryEventLogSql, new { InProgress = EventStateEnum.InProgress, NotPublished = EventStateEnum.NotPublished}).ConfigureAwait(false);
        }

        public async Task SaveEventAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string InsertEventLogSql =
$@"INSERT INTO {_tableName} 
(""EventId"", ""CreationTime"", ""EventTypeName"", ""Content"", ""State"", ""TimesSent"") 
VALUES (@EventId, @CreationTime, @EventTypeName, @Content, @State, @TimesSent); ";

            var eventLogEntry = new EventLogEntry(@event);

            await Connection.ExecuteAsync(
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
                    transaction: Transaction).ConfigureAwait(false);
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

            var eventLogEntry = await Connection.QuerySingleAsync<EventLogEntry>(
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

            await Connection.ExecuteAsync(
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
