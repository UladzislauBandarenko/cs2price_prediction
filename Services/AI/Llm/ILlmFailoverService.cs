using System.Threading.Tasks;

namespace cs2price_prediction.Services.AI.Llm
{
    public interface ILlmFailoverService
    {
        Task<string> QueryWithPriorityAsync(string prompt, LlmPriority priority);
    }
}
