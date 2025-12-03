using System.Collections.Generic;

namespace cs2price_prediction.Domain.Meta
{
    public class Weapon
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // "AK-47", "AWP", "Karambit"

        public int? WeaponTypeId { get; set; }
        public WeaponType? WeaponType { get; set; }

        public ICollection<Skin> Skins { get; set; } = new List<Skin>();
    }
}
