using System.Threading.Tasks;

namespace cs2price_prediction.Services.AI.Llm
{
    public interface ILLMClient
    {
        Task<string> QueryAsync(string prompt, string? modelOverride = null);
    }
}
