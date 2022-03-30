namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISagaRepository
    {
        /// <summary>
        /// Persist the saga and the sgag steps to the database.
        /// </summary>
        /// <param name="saga">the data of the saga.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public Task AddSagaAsync(Saga saga, CancellationToken cancellationToken = default);

        /// <summary>
        /// Uodate the saga and the sgag steps.
        /// </summary>
        /// <param name="saga">the data of the saga.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task UpdateSagaByIdAsync(Saga saga, CancellationToken cancellationToken = default);
    }
}
