using System.Data;
using System.Threading.Tasks;

namespace Coast.Core.EventBus
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData, IDbTransaction transaction = null);
    }
}
