namespace cs2price_prediction.Services.Stickers
{
    public class StickerFeatures
    {
        public int StickersCount { get; set; }
        public double StickersTotalValue { get; set; }
        public double StickersAvgValue { get; set; }
        public double StickersMaxValue { get; set; }

        public double Slot0Price { get; set; }
        public double Slot1Price { get; set; }
        public double Slot2Price { get; set; }
        public double Slot3Price { get; set; }
    }
}
