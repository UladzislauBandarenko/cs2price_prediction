using cs2price_prediction.Domain.Meta;

namespace cs2price_prediction.Domain.Patterns
{
    public class CaseHardenedGunPattern
    {
        public int Id { get; set; }
        public int SkinId { get; set; }
        public int Pattern { get; set; }          // 1..1000

        public double PlaysideBlue { get; set; }
        public double BacksideBlue { get; set; }

        public Skin Skin { get; set; } = null!;
    }
}
