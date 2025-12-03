using System.Collections.Generic;

namespace cs2price_prediction.Domain.Meta
{
    public class WeaponType
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!; // "rifle", "knife"
        public string Name { get; set; } = null!; // "Rifle", "Knife"

        public ICollection<Weapon> Weapons { get; set; } = new List<Weapon>();
    }
}
