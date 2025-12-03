using System.Text.Json.Serialization;

namespace cs2price_prediction.DTOs.Prediction
{
    public class MlPredictionResponse
    {
        [JsonPropertyName("predicted_price")]
        public double PredictedPrice { get; set; }

        // Если хочешь забирать ещё и фичи стикеров из ответа ML:
        [JsonPropertyName("stickers_features")]
        public MlStickerFeaturesResponse? StickersFeatures { get; set; }
    }

    public class MlStickerFeaturesResponse
    {
        [JsonPropertyName("stickers_count")]
        public int StickersCount { get; set; }

        [JsonPropertyName("stickers_total_value")]
        public double StickersTotalValue { get; set; }

        [JsonPropertyName("stickers_avg_value")]
        public double StickersAvgValue { get; set; }

        [JsonPropertyName("stickers_max_value")]
        public double StickersMaxValue { get; set; }
    }
}
