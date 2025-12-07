using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.AI.FadeKnife
{
    /// <summary>
    /// DTO для AI-объяснения цены Fade ножей.
    /// Никакой информации о стикерах.
    /// </summary>
    public class AiFadeKnivesRequest
    {
        [JsonPropertyName("weapon")]
        public string Weapon { get; set; } = "";

        [JsonPropertyName("skin")]
        public string Skin { get; set; } = "";

        [JsonPropertyName("wear")]
        public string Wear { get; set; } = "";

        [JsonPropertyName("float")]
        public double Float { get; set; }

        [JsonPropertyName("pattern")]
        public int Pattern { get; set; }

        [JsonPropertyName("stattrak")]
        public int Stattrak { get; set; }

        [JsonPropertyName("fade_percentage")]
        public double FadePercentage { get; set; }

        [JsonPropertyName("fade_rank")]
        public double FadeRank { get; set; }

        [JsonPropertyName("predicted_price")]
        public double PredictedPrice { get; set; }
    }
}
