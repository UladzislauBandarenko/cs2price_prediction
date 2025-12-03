using cs2price_prediction.Domain.Meta;

namespace cs2price_prediction.Domain.Patterns
{
    public class FadeGunPattern
    {
        public int Id { get; set; }
        public int SkinId { get; set; }
        public int Pattern { get; set; }

        public double FadePercentage { get; set; }
        public double FadeRank { get; set; }

        public Skin Skin { get; set; } = null!;
    }
}
