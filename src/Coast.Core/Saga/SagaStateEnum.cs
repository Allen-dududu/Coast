namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum SagaStateEnum
    {
        Created = 1,
        Started = 2,
        Aborting = 3,
        Aborted = 4,
        Completed = 5
    }
}
