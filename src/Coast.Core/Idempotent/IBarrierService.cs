namespace Coast.Core
{
    using Microsoft.Extensions.Logging;

    public interface IBarrierService
    {
        BranchBarrier CreateBranchBarrier(string transType, string gid, string branchID, string op, ILogger? logger = null);

        BranchBarrier CreateBranchBarrier(SagaEvent @event, ILogger? logger = null);

    }
}
