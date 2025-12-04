namespace cs2price_prediction.DTOs.Admin.Weapons
{
    public class UpdateWeaponDto
    {
        public int WeaponTypeId { get; set; }
        public string Name { get; set; } = default!;
    }
}
