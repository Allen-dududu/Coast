namespace Coast.Core
{
    using System.Collections.Generic;

    public abstract class EventRequestBody
    {
        public IDictionary<string, string> Headers { get; set; }
    }
}
