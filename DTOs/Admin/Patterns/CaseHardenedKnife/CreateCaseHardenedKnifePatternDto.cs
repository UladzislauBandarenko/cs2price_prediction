namespace cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedKnife
{
    public class CreateCaseHardenedKnifePatternDto
    {
        public int SkinId { get; set; }
        public int Pattern { get; set; }

        public double BacksideBlue { get; set; }
        public double? BacksidePurple { get; set; }
        public double? BacksideGold { get; set; }

        public double PlaysideBlue { get; set; }
        public double? PlaysidePurple { get; set; }
        public double? PlaysideGold { get; set; }
    }
}
