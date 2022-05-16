namespace Coast.Core
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISagaManager
    {
        Task<Saga> CreateAsync(IEnumerable<EventRequestBody> steps = default, CancellationToken cancellationToken = default);
        Task StartAsync(Saga saga, CancellationToken cancellationToken = default);
        //Task TransitAsync(SagaEvent sagaEvent, IDbConnection conn, IDbTransaction transaction, CancellationToken cancellationToken = default);
    }
}