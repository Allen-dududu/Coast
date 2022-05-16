namespace Coast.Core.MigrationManager
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICoastDBInitializer
    {
        Task InitializeAsync(string schema, CancellationToken cancellationToken);
    }
}
