namespace cs2price_prediction.DTOs.Admin.Stickers
{
    public class CreateStickerDto
    {
        public string Name { get; set; } = default!;

        // Можно передать референсную цену (опционально)
        public double? ReferencePrice { get; set; }
    }
}
