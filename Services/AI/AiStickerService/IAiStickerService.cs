using System.Collections.Generic;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.AI.StickersDtoForAI;

namespace cs2price_prediction.Services.AI.AiStickerService
{
    public interface IAiStickerService
    {
        /// <summary>
        /// stickerIds[0] -> Slot0Price + StickerSlot1Name
        /// stickerIds[1] -> Slot1Price + StickerSlot2Name
        /// stickerIds[2] -> Slot2Price + StickerSlot3Name
        /// stickerIds[3] -> Slot3Price + StickerSlot4Name
        /// </summary>
        Task<StickersDtoForAI> BuildStickersDtoForAiAsync(IReadOnlyList<int> stickerIds);
    }
}
