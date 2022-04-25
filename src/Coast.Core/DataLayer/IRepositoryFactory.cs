namespace Coast.Core.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using Coast.Core.DataLayer;
    using Coast.Core.EventBus.IntegrationEventLog;

    public interface IRepositoryFactory
    {
        IWapperSession OpenSession(IDbConnection dbConnection = null);
    }
}
