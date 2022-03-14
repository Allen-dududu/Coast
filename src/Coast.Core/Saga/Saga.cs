namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Saga
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public 

        public ICollection<SageStep> sageSteps { get; set; }
    }
}
