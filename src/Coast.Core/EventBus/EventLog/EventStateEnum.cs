namespace Coast.Core.EventBus.EventLog
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum EventStateEnum
    {
        NotPublished = 0,
        InProgress = 1,
        Published = 2,
        PublishedFailed = 3
    }
}
