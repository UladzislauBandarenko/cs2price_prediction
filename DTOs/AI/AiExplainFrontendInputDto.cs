using System.Collections.Generic;

namespace cs2price_prediction.DTOs.AI
{

    public class AiExplainFrontendInputDto
    {
        public double PredictedPrice { get; set; }

        public int SkinId { get; set; }
        public int WearTierId { get; set; }
        public double FloatValue { get; set; }
        public bool IsStattrak { get; set; }
        public int Pattern { get; set; }
        public List<int> Stickers { get; set; } = new();
    }
}
