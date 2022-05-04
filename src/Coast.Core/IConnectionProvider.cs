using System.Data;

namespace Coast.Core
{
    public interface IConnectionProvider
    {
        IDbConnection GetAdventureWorksConnection();
    }
}
