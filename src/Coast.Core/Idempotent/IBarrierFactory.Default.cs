namespace Coast.Core.Barrier
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;

    public class DefaultBarrierFactory : IBarrierFactory
    {
        public Barrier CreateBranchBarrier(string transType, string gid, string branchID, string op, ILogger? logger = null)
        {
            throw new NotImplementedException();
        }
    }
}
