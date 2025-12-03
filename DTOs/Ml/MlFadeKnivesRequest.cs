using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.Ml
{
    public class MlFadeKnivesRequest
    {
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

        [JsonPropertyName("weapon")]
        public string Weapon { get; set; } = "";

        [JsonPropertyName("skin")]
        public string Skin { get; set; } = "";

        [JsonPropertyName("wear")]
        public string Wear { get; set; } = "";
    }
}
