using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.Ml
{
    public class MlCaseHardenedKnifeRequest
    {
        [JsonPropertyName("float")]
        public double Float { get; set; }

        [JsonPropertyName("pattern")]
        public int Pattern { get; set; }

        [JsonPropertyName("stattrak")]
        public int Stattrak { get; set; }

        [JsonPropertyName("backside_blue")]
        public double BacksideBlue { get; set; }

        [JsonPropertyName("backside_purple")]
        public double BacksidePurple { get; set; }

        [JsonPropertyName("backside_gold")]
        public double BacksideGold { get; set; }

        [JsonPropertyName("playside_blue")]
        public double PlaysideBlue { get; set; }

        [JsonPropertyName("playside_purple")]
        public double PlaysidePurple { get; set; }

        [JsonPropertyName("playside_gold")]
        public double PlaysideGold { get; set; }

        [JsonPropertyName("weapon")]
        public string Weapon { get; set; } = "";

        [JsonPropertyName("skin")]
        public string Skin { get; set; } = "";

        [JsonPropertyName("wear")]
        public string Wear { get; set; } = "";
    }
}
