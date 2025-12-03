namespace cs2price_prediction.Domain.Meta
{
    public class WearTier
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // "Factory New", "Minimal Wear"
        public ICollection<SkinWearTier> SkinWearTiers { get; set; }= new List<SkinWearTier>();

    }
}
