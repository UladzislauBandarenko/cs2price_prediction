using System.Threading.Tasks;
using cs2price_prediction.DTOs.AI;
using cs2price_prediction.Services.AI.AiExplanation;
using cs2price_prediction.Services.AI.Llm;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiExplanationController : ControllerBase
    {
        private readonly IAiExplanationService _aiExplanationService;

        public AiExplanationController(IAiExplanationService aiExplanationService)
        {
            _aiExplanationService = aiExplanationService;
        }

        /// <summary>
        /// Main explanation endpoint.
        /// Priority order for LLMs: gpt-4o-mini first → fallback to gpt-4.1-mini.
        /// </summary>
        [HttpPost("explain")]
        public async Task<IActionResult> Explain([FromBody] AiExplainFrontendInputDto dto)
        {
            return await _aiExplanationService.ExplainAsync(dto, LlmPriority.MiniThenGpt41);
        }
    }
}
