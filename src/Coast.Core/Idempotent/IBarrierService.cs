namespace Coast.Core
{
    using Microsoft.Extensions.Logging;

    public interface IBarrierService
    {
        BranchBarrier CreateBranchBarrier(SagaEvent @event, ILogger? logger = null);
    }
}
