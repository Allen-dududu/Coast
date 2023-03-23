namespace Coast.Core.Util
{
    public static class CoastConstant
    {
        internal static string DomainName { get; set; }

        internal static string WorkerId { get; set; }

        public const string CallBackEventSuffix = ".CallBack.Coast";

        public const string QueueNameSuffix = ".Queue.Coast";

        public const string DeadQueueName = "DeadQueue.Coast";
    }
}
