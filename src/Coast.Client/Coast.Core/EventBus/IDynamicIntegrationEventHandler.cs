using System.Threading.Tasks;

namespace Coast.Core.EventBus
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
