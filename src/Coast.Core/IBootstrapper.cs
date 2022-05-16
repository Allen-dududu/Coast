namespace Coast.Core
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents bootstrapping logic. For example, adding initial state to the storage or querying certain entities.
    /// </summary>
    internal interface IBootstrapper
    {
        Task BootstrapAsync();
    }
}
