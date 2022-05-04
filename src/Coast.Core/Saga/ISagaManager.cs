namespace Coast.Core
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISagaManager
    {
        Task<Saga> CreateAsync(IEnumerable<IEventRequestBody> steps = null, CancellationToken cancellationToken = default);
        Task StartAsync(Saga saga, CancellationToken cancellationToken = default);
        Task TransitAsync(SagaEvent sagaEvent, IDbTransaction transaction = null, CancellationToken cancellationToken = default);
    }
}