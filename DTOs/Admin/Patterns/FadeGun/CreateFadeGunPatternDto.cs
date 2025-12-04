namespace cs2price_prediction.DTOs.Admin.Patterns.FadeGun
{
    public class CreateFadeGunPatternDto
    {
        public int SkinId { get; set; }
        public int Pattern { get; set; }
        public double FadePercentage { get; set; }
        public double FadeRank { get; set; }
    }
}
