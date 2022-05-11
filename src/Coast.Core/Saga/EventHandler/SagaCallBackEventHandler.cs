namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;
    using Microsoft.Extensions.Logging;

    public class SagaCallBackEventHandler : ISagaHandler
    {
        private readonly ISagaManager _sagaManager;
        private readonly IBarrierService _barrierService;
        private readonly ILogger<SagaCallBackEventHandler> _logger;
        private readonly IConnectionProvider _connectionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaCallBackEventHandler"/> class.
        /// </summary>
        public SagaCallBackEventHandler(
            ISagaManager sagaManager,
            IBarrierService barrierService,
            ILogger<SagaCallBackEventHandler> logger,
            IConnectionProvider connectionProvider)
        {
            _sagaManager = sagaManager;
            _barrierService = barrierService;
            _logger = logger;
            _connectionProvider = connectionProvider;
        }

        public Task CancelAsync(string @event)
        {
            throw new NotImplementedException();
        }

        public async Task CommitAsync(string @event)
        {
        }
    }
}
