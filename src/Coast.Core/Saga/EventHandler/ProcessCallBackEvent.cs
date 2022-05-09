namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class ProcessCallBackEvent : IProcessCallBackEvent
    {
        private readonly ISagaManager _sagaManager;
        private readonly IBarrierService _barrierService;
        private readonly ILogger<ProcessCallBackEvent> _logger;
        private readonly IConnectionProvider _connectionProvider;

        public ProcessCallBackEvent(ISagaManager sagaManager, IBarrierService barrierService, ILogger<ProcessCallBackEvent> logger, IConnectionProvider connectionProvider)
        {
            _sagaManager = sagaManager;
            _barrierService = barrierService;
            _logger = logger;
            _connectionProvider = connectionProvider;
        }

        public async Task ProcessEventAsync(SagaEvent @event)
        {
            var barrier = _barrierService.CreateBranchBarrier(@event, _logger);

            using var connection = _connectionProvider.OpenConnection();
            await barrier.Call(connection, async (connection, trans) => await _sagaManager.TransitAsync(@event, connection, trans));
        }
    }
}
