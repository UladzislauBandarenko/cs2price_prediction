using System.Collections.Generic;
using System.Threading.Tasks;

namespace cs2price_prediction.Services.Stickers
{
    public interface IStickerService
    {
        Task<StickerFeatures> CalculateFeaturesAsync(IReadOnlyCollection<int> stickerIds);
    }
}
