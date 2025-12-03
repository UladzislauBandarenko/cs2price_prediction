using System.Collections.Generic;

namespace cs2price_prediction.Domain.Patterns
{
    public class DopplerPhase
    {
        public int Id { get; set; }

        // "Ruby", "Sapphire", "Black Pearl", "Phase 1", ...
        public string Name { get; set; } = null!;

        public ICollection<DopplerSkinPhase> DopplerSkinPhases { get; set; }
            = new List<DopplerSkinPhase>();
    }
}
