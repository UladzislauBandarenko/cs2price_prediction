using cs2price_prediction.Domain.Meta;

namespace cs2price_prediction.Domain.Patterns
{
    public class CaseHardenedKnifePattern
    {
        public int Id { get; set; }
        public int SkinId { get; set; }
        public int Pattern { get; set; }

        public double BacksideBlue { get; set; }
        public double? BacksidePurple { get; set; }
        public double? BacksideGold { get; set; }

        public double PlaysideBlue { get; set; }
        public double? PlaysidePurple { get; set; }
        public double? PlaysideGold { get; set; }

        public Skin Skin { get; set; } = null!;
    }
}
