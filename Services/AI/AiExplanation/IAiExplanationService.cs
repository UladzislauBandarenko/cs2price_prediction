using System.Threading.Tasks;
using cs2price_prediction.DTOs.AI;
using cs2price_prediction.Services.AI.Llm;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Services.AI.AiExplanation
{
    public interface IAiExplanationService
    {
        Task<IActionResult> ExplainAsync(AiExplainFrontendInputDto dto, LlmPriority priority);
    }
}
