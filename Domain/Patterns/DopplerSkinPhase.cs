using cs2price_prediction.Domain.Meta;

namespace cs2price_prediction.Domain.Patterns
{
    public class DopplerSkinPhase
    {
        public int Id { get; set; }

        public int SkinId { get; set; }
        public int PhaseId { get; set; }

        public Skin Skin { get; set; } = null!;
        public DopplerPhase Phase { get; set; } = null!;
    }
}
