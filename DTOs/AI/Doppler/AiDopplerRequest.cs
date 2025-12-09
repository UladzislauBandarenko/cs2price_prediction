using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.AI.Doppler
{
    /// <summary>
    /// DTO для AI-объяснения цены доплер-ножей.
    /// Полностью без наклеек.
    /// </summary>
    public class AiDopplerRequest
    {
        [JsonPropertyName("weapon")]
        public string Weapon { get; set; } = "";

        [JsonPropertyName("skin")]
        public string Skin { get; set; } = "";

        [JsonPropertyName("wear")]
        public string Wear { get; set; } = "";

        [JsonPropertyName("phase")]
        public string Phase { get; set; } = "";

        [JsonPropertyName("float")]
        public double Float { get; set; }

        [JsonPropertyName("stattrak")]
        public int Stattrak { get; set; }

        [JsonPropertyName("predicted_price")]
        public double PredictedPrice { get; set; }
    }
}
