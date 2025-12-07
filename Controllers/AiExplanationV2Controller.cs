using System.Threading.Tasks;
using cs2price_prediction.DTOs.AI;
using cs2price_prediction.Services.AI.AiExplanation;
using cs2price_prediction.Services.AI.Llm;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiExplanationV2Controller : ControllerBase
    {
        private readonly IAiExplanationService _aiExplanationService;

        public AiExplanationV2Controller(IAiExplanationService aiExplanationService)
        {
            _aiExplanationService = aiExplanationService;
        }

        /// <summary>
        /// Alternative explanation endpoint that uses a prioritized model order.
        /// Priority: gpt-4.1-mini → fallback to gpt-4o-mini if the primary model fails.
        /// </summary>
        [HttpPost("explain-v2")]
        public async Task<IActionResult> ExplainV2([FromBody] AiExplainFrontendInputDto dto)
        {
            return await _aiExplanationService.ExplainAsync(dto, LlmPriority.Gpt41ThenMini);
        }
    }
}
