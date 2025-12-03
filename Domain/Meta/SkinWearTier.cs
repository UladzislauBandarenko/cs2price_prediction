using System.Collections.Generic;

namespace cs2price_prediction.Domain.Meta
{
    public class SkinWearTier
    {
        public int Id { get; set; }

        public int SkinId { get; set; }
        public Skin Skin { get; set; } = null!;

        public int WearTierId { get; set; }
        public WearTier WearTier { get; set; } = null!;
    }
}
