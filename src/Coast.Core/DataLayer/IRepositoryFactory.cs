namespace Coast.Core.DataLayer
{
    using System.Data;

    public interface IRepositoryFactory
    {
        IWapperSession OpenSession(IDbConnection dbConnection = null);

        IDbConnection OpenConnection();
    }
}
