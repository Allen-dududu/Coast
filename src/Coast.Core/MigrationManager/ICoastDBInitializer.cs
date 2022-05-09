namespace Coast.Core.MigrationManager
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICoastDBInitializer
    {
        Task InitializeAsync(string schema, CancellationToken cancellationToken);
    }
}
