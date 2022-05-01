using System.Data;

namespace Coast.PostgreSql.Connection
{
    public interface IConnectionProvider
    {
        IDbConnection GetAdventureWorksConnection();
    }
}
