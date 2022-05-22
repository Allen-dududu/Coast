namespace Coast.Core.Processor
{
    using System;
    using System.Threading;

    internal interface IProcessingServer : IDisposable
    {
        void Start(CancellationToken stoppingToken);
    }
}
