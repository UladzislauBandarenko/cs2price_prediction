namespace cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedGun
{
    public class CreateCaseHardenedGunPatternDto
    {
        public int SkinId { get; set; }
        public int Pattern { get; set; }          // 1..1000
        public double PlaysideBlue { get; set; }
        public double BacksideBlue { get; set; }
    }
}
