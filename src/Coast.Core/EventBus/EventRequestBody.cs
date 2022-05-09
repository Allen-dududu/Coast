namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class EventRequestBody
    {
        public IDictionary<string, string> Headers { get; set; }
    }
}
