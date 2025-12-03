using System.Threading.Tasks;
using cs2price_prediction.DTOs.Prediction;
using cs2price_prediction.Services.Prediction;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers
{
    [ApiController]
    [Route("api/predict")]
    public class PredictionController : ControllerBase
    {
        private readonly IPredictionService _predictionService;

        public PredictionController(IPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpPost]
        public Task<IActionResult> Predict([FromBody] PredictionRequestDto dto)
        {
            return _predictionService.PredictAsync(dto);
        }
    }
}
