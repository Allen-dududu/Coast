namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;

    public class SagaCallBackEventHandler : IDynamicIntegrationEventHandler
    {
        private readonly ISagaManager _sagaManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaCallBackEventHandler"/> class.
        /// </summary>
        /// <param name="sagaManager">saga manager.</param>
        public SagaCallBackEventHandler(ISagaManager sagaManager)
        {
            _sagaManager = sagaManager;
        }

        /// <inheritdoc/>
        public async Task Handle(dynamic eventData, IDbTransaction transaction = null)
        {
            await _sagaManager.TransitAsync(eventData, transaction);
        }
    }
}
