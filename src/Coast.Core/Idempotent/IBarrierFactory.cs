namespace Coast.Core.Barrier
{
    using Microsoft.Extensions.Logging;

    internal interface IBarrierFactory
    {
        Barrier CreateBranchBarrier(string transType, string gid, string branchID, string op, ILogger? logger = null);
    }
}
