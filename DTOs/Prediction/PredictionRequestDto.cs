namespace cs2price_prediction.DTOs.Prediction
{
    public class PredictionRequestDto
    {
        public int SkinId { get; set; }
        public int WearTierId { get; set; }
        public double FloatValue { get; set; }

        // true/false c фронта, в ML уйдёт 1/0
        public bool IsStattrak { get; set; }

        // Номер паттерна (1..1000), не Id строки
        public int Pattern { get; set; }

        // До 4-х id стикеров (может быть null или пустой)
        public List<int>? Stickers { get; set; }
    }
}
