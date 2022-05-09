namespace Coast.Core
{
    using System.Data;

    public interface IConnectionProvider
    {
        IDbConnection OpenConnection();
    }
}
