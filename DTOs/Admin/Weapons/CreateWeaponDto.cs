namespace cs2price_prediction.DTOs.Admin.Weapons
{
    public class CreateWeaponDto
    {
        public int WeaponTypeId { get; set; }
        public string Name { get; set; } = default!;
    }
}
