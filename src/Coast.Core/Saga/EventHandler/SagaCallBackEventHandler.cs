namespace Coast.Core
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class SagaCallBackEventHandler : ISagaHandler
    {
        private readonly ISagaManager _sagaManager;
        private readonly IBarrierService _barrierService;
        private readonly ILogger<SagaCallBackEventHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaCallBackEventHandler"/> class.
        /// </summary>
        public SagaCallBackEventHandler(
            ISagaManager sagaManager,
            IBarrierService barrierService,
            ILogger<SagaCallBackEventHandler> logger)
        {
            _sagaManager = sagaManager;
            _barrierService = barrierService;
            _logger = logger;
        }

        public Task CancelAsync(string @event)
        {
            throw new NotImplementedException();
        }

        public async Task CommitAsync(string @event)
        {
            throw new NotImplementedException();
        }
    }
}
