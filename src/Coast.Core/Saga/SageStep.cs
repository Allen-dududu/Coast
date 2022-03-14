namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class SageStep
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string HandleEvent { get; set; }

        public string ConpensationHandleEvent { get; set; }

        public ISagaRequestBody RequestBody { get; set; }
    }
}
