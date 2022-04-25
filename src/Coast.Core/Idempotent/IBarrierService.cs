namespace Coast.Core
{
    using Microsoft.Extensions.Logging;

    internal interface IBarrierService
    {
        Barrier CreateBranchBarrier(string transType, string gid, string branchID, string op, ILogger? logger = null);
    }
}
