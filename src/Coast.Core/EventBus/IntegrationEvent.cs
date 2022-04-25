namespace Coast.Core.EventBus
{
    using System;
    using Newtonsoft.Json;

    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = SnowflakeId.Default().NextId(); ;
            CreationDate = DateTime.UtcNow;
        }

        [JsonConstructor]
        public IntegrationEvent(long id, DateTime createDate)
        {
            Id = id;
            CreationDate = createDate;
        }

        [JsonProperty]
        public long Id { get; private set; }

        [JsonProperty]
        public DateTime CreationDate { get; private set; }

        [JsonProperty]
        public string EventName { get; set; }
    }
}
