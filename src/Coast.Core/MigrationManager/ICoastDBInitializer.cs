using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Coast.Core.MigrationManager
{
    public interface ICoastDBInitializer
    {
        Task InitializeAsync();
    }
}
