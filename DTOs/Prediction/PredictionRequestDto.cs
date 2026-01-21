namespace cs2price_prediction.DTOs.Prediction
{
    public class PredictionRequestDto
    {
        public int SkinId { get; set; }
        public int WearTierId { get; set; }
        public double FloatValue { get; set; }//0.00333333333333334444444

        // true/false 
        public bool IsStattrak { get; set; }

        // Patten ID
        public int? Pattern { get; set; }

        // 4s id stikers
        public List<int>? Stickers { get; set; }
    }
}
