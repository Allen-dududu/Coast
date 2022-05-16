namespace Coast.Core
{
    public enum SagaStepStateEnum
    {
        Awaiting = 0,
        Started = 1,
        Failed = 2,
        Succeeded = 3,
        Compensating = 4,
        Compensated = 5,
        Cancelled = 6
    }
}
