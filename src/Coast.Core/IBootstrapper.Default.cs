namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;
    using Coast.Core.MigrationManager;
    using Coast.Core.Processor;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class Bootstrapper : BackgroundService, IBootstrapper
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<Bootstrapper> _logger;

        private IEnumerable<IProcessingServer> _processors = default;

        public Bootstrapper(IServiceProvider serviceProvider, ILogger<Bootstrapper> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task BootstrapAsync()
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("### Coast background task is starting.");

            CheckRequirement();

            try
            {
                _processors = _serviceProvider.GetServices<IProcessingServer>();

                await _serviceProvider.GetRequiredService<ICoastDBInitializer>().InitializeAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Initializing the storage structure failed!");
                throw e;
            }

            var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
            var option = _serviceProvider.GetRequiredService<CoastOptions>();
            eventBus.SubscribeDynamic<SagaCallBackEventHandler>(option.DomainName);

            stoppingToken.Register(() =>
            {
                _logger.LogDebug("### Coast background task is stopping.");

                foreach (var item in _processors)
                {
                    try
                    {
                        item.Dispose();
                    }
                    catch (OperationCanceledException ex)
                    {
                        _logger.LogWarning($"Expected an OperationCanceledException. {ex.Message}");
                    }
                }
            });
        }

        private void CheckRequirement()
        {
            var eventBus = _serviceProvider.GetService<IEventBus>();
            if (eventBus == null)
            {
                throw new InvalidOperationException(
                  $"You must be config transport provider for Coast!" + Environment.NewLine +
                  $"==================================================================================" + Environment.NewLine +
                  $"========   eg: services.AddCosat( options => {{ options.UseRabbitMQ(...) }}); ========" + Environment.NewLine +
                  $"==================================================================================");
            }

            var dBInit = _serviceProvider.GetService<ICoastDBInitializer>();
            if (dBInit == null)
            {
                throw new InvalidOperationException(
                  $"You must be config storage provider for Coast!" + Environment.NewLine +
                  $"==================================================================================" + Environment.NewLine +
                  $"========   eg: services.AddCosat( options => {{ options.UsePostgreSql(...) }}); ========" + Environment.NewLine +
                  $"==================================================================================");
            }
        }
    }
}
