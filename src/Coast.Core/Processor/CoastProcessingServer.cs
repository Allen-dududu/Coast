﻿namespace Coast.Core.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class CoastProcessingServer : IProcessingServer
    {
        private readonly CancellationTokenSource _cts;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceProvider _provider;

        private Task? _compositeTask;
        private ProcessingContext _context = default!;
        private bool _disposed;

        public CoastProcessingServer(
            ILogger<CoastProcessingServer> logger,
            ILoggerFactory loggerFactory,
            IServiceProvider provider)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _provider = provider;
            _cts = new CancellationTokenSource();
        }

        public void Start(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => _cts.Cancel());

            _logger.LogInformation("Starting the processing server.");

            _context = new ProcessingContext(_provider, _cts.Token);

            var processorTasks = GetProcessors()
                .Select(InfiniteRetry)
                .Select(p => p.ProcessAsync(_context));
            _compositeTask = Task.WhenAll(processorTasks);
        }

        public void Pulse()
        {
            _logger.LogTrace("Pulsing the processor.");
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _disposed = true;

                _logger.LogInformation("Shutting down the processing server...");
                _cts.Cancel();

                _compositeTask?.Wait((int)TimeSpan.FromSeconds(10).TotalMilliseconds);
            }
            catch (AggregateException ex)
            {
                var innerEx = ex.InnerExceptions[0];
                if (!(innerEx is OperationCanceledException))
                {
                    _logger.LogWarning(ex, $"Expected an OperationCanceledException, but found '{ex.Message}'.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An exception was occurred when disposing.");
            }
            finally
            {
                _logger.LogInformation("### Coast shutdown!");
            }
        }

        private IProcessor InfiniteRetry(IProcessor inner)
        {
            return new InfiniteRetryProcessor(inner, _loggerFactory);
        }

        private IProcessor[] GetProcessors()
        {
            var returnedProcessors = new List<IProcessor>
            {
                _provider.GetRequiredService<MessageNeedToRetryProcessor>(),
            };

            return returnedProcessors.ToArray();
        }
    }
}
