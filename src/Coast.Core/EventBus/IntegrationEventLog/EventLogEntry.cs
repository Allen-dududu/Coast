namespace Coast.Core.EventBus.IntegrationEventLog
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
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
            EventTypeName = @event.EventName ?? @event.GetType().FullName;
            Content = JsonSerializer.Serialize(@event, @event.GetType(), new JsonSerializerOptions
            {
                WriteIndented = true
            });
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

        public EventLogEntry DeserializeJsonContent(Type type)
        {
            IntegrationEvent = JsonSerializer.Deserialize(Content, type, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }) as IntegrationEvent;
            return this;
        }
    }
}
