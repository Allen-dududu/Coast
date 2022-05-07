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

        public Task Cancel(SagaEvent @event)
        {
            throw new NotImplementedException();
        }

        public async Task Commit(SagaEvent @event)
        {
            var barrier = _barrierService.CreateBranchBarrier(@event, _logger);

            var connection = _connectionProvider.GetAdventureWorksConnection();
            await barrier.Call(connection, async (tx) => await _sagaManager.TransitAsync(@event, tx));
        }
    }
}
