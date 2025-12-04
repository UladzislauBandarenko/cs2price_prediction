namespace cs2price_prediction.DTOs.Admin.SkinWearTiers
{
    public class UpdateSkinWearTierDto
    {
        public int SkinId { get; set; }

        public int OldWearTierId { get; set; }

        public int NewWearTierId { get; set; }
    }
}
