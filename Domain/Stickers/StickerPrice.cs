namespace cs2price_prediction.Domain.Stickers
{
    public class StickerPrice
    {
        public int Id { get; set; }

        public int StickerId { get; set; }
        public double Price { get; set; }  // в USDT

        public Sticker Sticker { get; set; } = null!;
    }
}
