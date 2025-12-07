using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.AI.StickersDtoForAI
{
    public class StickersDtoForAI
    {
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
    }
}
