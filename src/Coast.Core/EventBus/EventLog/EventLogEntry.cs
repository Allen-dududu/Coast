namespace Coast.Core.EventBus.EventLog
{
    using System;
    using System.Linq;
    using System.Text.Json;

    public class EventLogEntry
    {
        public EventLogEntry()
        {

        }

        public EventLogEntry(IntegrationEvent @event)
        {
            EventId = @event.Id;
            CreationTime = @event.CreationDate;
            EventTypeName = @event.EventName;
            Content = JsonSerializer.Serialize(@event, @event.GetType());
            State = EventStateEnum.NotPublished;
            TimesSent = 0;
        }

        public long EventId { get; private set; }

        public string EventTypeName { get; private set; }

        public string EventTypeShortName => EventTypeName.Split('.')?.Last();

        public IntegrationEvent IntegrationEvent { get; private set; }

        public EventStateEnum State { get; set; }

        public int TimesSent { get; set; }

        public DateTime CreationTime { get; private set; }

        public string Content { get; private set; }
    }
}
