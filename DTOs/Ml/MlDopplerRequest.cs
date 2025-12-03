using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.Ml
{
    public class MlDopplerRequest
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
    }
}
