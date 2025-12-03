using System.Threading.Tasks;
using cs2price_prediction.DTOs.Prediction;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Services.Prediction
{
    public interface IPredictionService
    {
        Task<IActionResult> PredictAsync(PredictionRequestDto dto);
    }
}
