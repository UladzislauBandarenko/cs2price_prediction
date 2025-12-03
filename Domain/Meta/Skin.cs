using cs2price_prediction.Domain.Patterns;
using System.Collections.Generic;

namespace cs2price_prediction.Domain.Meta
{
    public class Skin
    {
        public int Id { get; set; }

        public int WeaponId { get; set; }

        public string Name { get; set; } = null!;
        public string PatternStyle { get; set; } = null!;

        public Weapon Weapon { get; set; } = null!;

        public ICollection<CaseHardenedGunPattern> CaseHardenedGunPatterns { get; set; }
            = new List<CaseHardenedGunPattern>();

        public ICollection<CaseHardenedKnifePattern> CaseHardenedKnifePatterns { get; set; }
            = new List<CaseHardenedKnifePattern>();

        public ICollection<FadeGunPattern> FadeGunPatterns { get; set; }
            = new List<FadeGunPattern>();

        public ICollection<FadeKnifePattern> FadeKnifePatterns { get; set; }
            = new List<FadeKnifePattern>();

        public ICollection<DopplerSkinPhase> DopplerSkinPhases { get; set; }
            = new List<DopplerSkinPhase>();

        public ICollection<SkinWearTier> SkinWearTiers { get; set; }
            = new List<SkinWearTier>();

    }
}
