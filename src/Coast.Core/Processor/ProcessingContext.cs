namespace Coast.Core.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    public class ProcessingContext : IDisposable
    {
        private IServiceScope? _scope;

        private ProcessingContext(ProcessingContext other)
        {
            Provider = other.Provider;
            CancellationToken = other.CancellationToken;
        }

        public ProcessingContext(
            IServiceProvider provider,
            CancellationToken cancellationToken)
        {
            Provider = provider;
            CancellationToken = cancellationToken;
        }

        public IServiceProvider Provider { get; private set; }

        public CancellationToken CancellationToken { get; }

        public bool IsStopping => CancellationToken.IsCancellationRequested;

        public void Dispose()
        {
            _scope?.Dispose();
        }

        public void ThrowIfStopping()
        {
            CancellationToken.ThrowIfCancellationRequested();
        }

        public ProcessingContext CreateScope()
        {
            var serviceScope = Provider.CreateScope();

            return new ProcessingContext(this)
            {
                _scope = serviceScope,
                Provider = serviceScope.ServiceProvider
            };
        }

        public Task WaitAsync(TimeSpan timeout)
        {
            return Task.Delay(timeout, CancellationToken);
        }
    }
}
