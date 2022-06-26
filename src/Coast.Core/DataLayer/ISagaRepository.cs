namespace Coast.Core
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISagaRepository
    {
        /// <summary>
        /// only persist the saga to the database.
        /// </summary>
        /// <param name="saga">the data of the saga.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public Task SaveSagaAsync(Saga saga, CancellationToken cancellationToken = default);

        /// <summary>
        ///  only the sgag steps to the database.
        /// </summary>
        /// <param name="saga">the data of the saga.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public Task SaveSagaStepsAsync(Saga saga, CancellationToken cancellationToken = default);

        /// <summary>
        /// Uodate the saga and the sgag steps.
        /// </summary>
        /// <param name="saga">the data of the saga.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task UpdateSagaAsync(Saga saga, CancellationToken cancellationToken = default);

        /// <summary>
        /// GetSaga by saga id.
        /// </summary>
        /// <param name="sagaId">the id of saga.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>saga instance.</returns>
        public Task<Saga> GetSagaByIdAsync(long sagaId, CancellationToken cancellationToken = default);
    }
}
