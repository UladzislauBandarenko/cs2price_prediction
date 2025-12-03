using System.Collections.Generic;

namespace cs2price_prediction.Domain.Stickers
{
    public class Sticker
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // "Sticker | Crown (Foil)"

        public ICollection<StickerPrice> Prices { get; set; }
            = new List<StickerPrice>();
    }
}
