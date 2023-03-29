namespace Coast.Core
{
    using System.Collections.Generic;

    public abstract class EventRequestBody
    {
        public IDictionary<string, string> Body { get; set; }
    }
}
