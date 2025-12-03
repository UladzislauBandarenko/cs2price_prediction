using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.Ml
{
    public class MlChGunsRequest
    {
        // ---------- categorical ----------
        [JsonPropertyName("weapon")]
        public string Weapon { get; set; } = "";

        [JsonPropertyName("skin")]
        public string Skin { get; set; } = "";

        [JsonPropertyName("wear")]
        public string Wear { get; set; } = "";

        [JsonPropertyName("pattern_style")]
        public string PatternStyle { get; set; } = "";

        // ---------- numeric ----------
        [JsonPropertyName("float")]
        public double Float { get; set; }

        [JsonPropertyName("pattern")]
        public int Pattern { get; set; }

        [JsonPropertyName("stattrak")]
        public int Stattrak { get; set; }

        [JsonPropertyName("backside_blue")]
        public double BacksideBlue { get; set; }

        [JsonPropertyName("playside_blue")]
        public double PlaysideBlue { get; set; }

        // ---------- sticker features ----------
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

        // ---------- engineered ----------
        [JsonPropertyName("blue_score")]
        public double BlueScore { get; set; }

        [JsonPropertyName("blue_tier")]
        public double BlueTier { get; set; }
    }
}
