using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Coast.Core.Test
{
    public class CallBackEventServiceTests
    {
        [Fact]
        public void GetLockId_NoException ()
        {
            var x = long.Parse(long.MaxValue.ToString()) + int.MaxValue;
            var y = long.Parse(long.MaxValue.ToString()) + int.MaxValue;
            Assert.True(x == y);
        }
    }
}
