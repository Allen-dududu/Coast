namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum SagaStatusEnum
    {
        Creating = 0,
        Created = 1,
        Started = 2,
        Aborting = 3,
        Aborted = 4,
        Completed = 5
    }
}
