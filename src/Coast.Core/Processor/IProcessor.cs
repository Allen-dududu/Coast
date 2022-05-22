using System.Threading.Tasks;

namespace Coast.Core.Processor
{
    public interface IProcessor
    {
        Task ProcessAsync(ProcessingContext context);
    }
}