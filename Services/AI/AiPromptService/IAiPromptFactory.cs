using cs2price_prediction.DTOs.AI.CaseHardenedKnife;
using cs2price_prediction.DTOs.AI.ChGuns;
using cs2price_prediction.DTOs.AI.Doppler;
using cs2price_prediction.DTOs.AI.FadeGun;
using cs2price_prediction.DTOs.AI.FadeKnife;
using cs2price_prediction.DTOs.AI.FloatGuns;

namespace cs2price_prediction.Services.AI.AiPromptService
{
    public interface IAiPromptFactory
    {
        string BuildCaseHardenedKnifePrompt(AiCaseHardenedKnifeRequest r);
        string BuildChGunsPrompt(AiChGunsRequest r);
        string BuildDopplerPrompt(AiDopplerRequest r);
        string BuildFadeGunsPrompt(AiFadeGunsRequest r);
        string BuildFadeKnivesPrompt(AiFadeKnivesRequest r);
        string BuildFloatGunsPrompt(AiFloatSensitiveGunsRequest r);
    }
}
