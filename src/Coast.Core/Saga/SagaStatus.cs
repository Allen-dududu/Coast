namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum SagaStatus
    {
        Created = 0,
        Started = 1,
        Aborting = 2,
        Aborted = 3,
        Completed = 4
    }
}
