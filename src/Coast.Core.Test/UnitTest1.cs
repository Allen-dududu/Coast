using Xunit;

namespace Coast.Core.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var x = "Alert";
            var y = "tenant";

            var worerId = x.GetHashCode() % 1023;
            var workerid2 = y.GetHashCode() % 1023;
            Assert.NotEqual(worerId, workerid2);
        }
    }
}