namespace cs2price_prediction.DTOs.Admin.Skins
{
    public class UpdateSkinDto
    {
        public int WeaponId { get; set; }
        public string Name { get; set; } = default!;
        public string? PatternStyle { get; set; }
    }
}
