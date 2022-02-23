using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coast.Core.Barrier
{
    internal interface IBarrierFactory
    {
        Barrier CreateBranchBarrier(string transType, string gid, string branchID, string op, ILogger? logger = null);
    }
}
