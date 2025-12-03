using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.Ml
{
    public class MlFloatSensitiveGunsRequest
    {
        [JsonPropertyName("float")]
        public double Float { get; set; }

        [JsonPropertyName("stattrak")]
        public int Stattrak { get; set; }

        [JsonPropertyName("stickers_count")]
        public int StickersCount { get; set; }

        [JsonPropertyName("stickers_total_value")]
        public double StickersTotalValue { get; set; }

        [JsonPropertyName("stickers_avg_value")]
        public double StickersAvgValue { get; set; }

        [JsonPropertyName("stickers_max_value")]
        public double StickersMaxValue { get; set; }

        [JsonPropertyName("slot0_price")]
        public double Slot0Price { get; set; }

        [JsonPropertyName("slot1_price")]
        public double Slot1Price { get; set; }

        [JsonPropertyName("slot2_price")]
        public double Slot2Price { get; set; }

        [JsonPropertyName("slot3_price")]
        public double Slot3Price { get; set; }

        [JsonPropertyName("weapon")]
        public string Weapon { get; set; } = "";

        [JsonPropertyName("skin")]
        public string Skin { get; set; } = "";

        [JsonPropertyName("wear")]
        public string Wear { get; set; } = "";
    }
}
