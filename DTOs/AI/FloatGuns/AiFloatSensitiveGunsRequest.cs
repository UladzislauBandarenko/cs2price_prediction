using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.AI.FloatGuns
{
    public class AiFloatSensitiveGunsRequest
    {
        [JsonPropertyName("float")]
        public double Float { get; set; }

        [JsonPropertyName("stattrak")]
        public int Stattrak { get; set; }

        [JsonPropertyName("weapon")]
        public string Weapon { get; set; } = "";

        [JsonPropertyName("skin")]
        public string Skin { get; set; } = "";

        [JsonPropertyName("wear")]
        public string Wear { get; set; } = "";

        [JsonPropertyName("slot0_price")]
        public double Slot0Price { get; set; }

        [JsonPropertyName("slot1_price")]
        public double Slot1Price { get; set; }

        [JsonPropertyName("slot2_price")]
        public double Slot2Price { get; set; }

        [JsonPropertyName("slot3_price")]
        public double Slot3Price { get; set; }

        [JsonPropertyName("sticker_slot1_name")]
        public string? StickerSlot1Name { get; set; }

        [JsonPropertyName("sticker_slot2_name")]
        public string? StickerSlot2Name { get; set; }

        [JsonPropertyName("sticker_slot3_name")]
        public string? StickerSlot3Name { get; set; }

        [JsonPropertyName("sticker_slot4_name")]
        public string? StickerSlot4Name { get; set; }

        [JsonPropertyName("predicted_price")]
        public double PredictedPrice { get; set; }
    }
}
