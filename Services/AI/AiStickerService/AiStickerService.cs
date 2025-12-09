using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.AI.StickersDtoForAI;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.AI.AiStickerService
{
    public class AiStickerService : IAiStickerService
    {
        private readonly AppDbContext _db;

        public AiStickerService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<StickersDtoForAI> BuildStickersDtoForAiAsync(IReadOnlyList<int> stickerIds)
        {
            var dto = new StickersDtoForAI();

            if (stickerIds == null || stickerIds.Count == 0)
                return dto;

            var uniqueIds = stickerIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (uniqueIds.Count == 0)
                return dto;

            var stickers = await _db.Stickers
                .Where(s => uniqueIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    Price = s.Prices
                        .OrderByDescending(p => p.Id)
                        .Select(p => p.Price)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var dict = stickers.ToDictionary(x => x.Id, x => new { x.Name, x.Price });

            void FillSlot(int slotIndex, int stickerIndex)
            {
                if (stickerIndex >= stickerIds.Count)
                    return;

                var stickerId = stickerIds[stickerIndex];
                if (stickerId <= 0)
                    return;

                if (!dict.TryGetValue(stickerId, out var info))
                    return;

                switch (slotIndex)
                {
                    case 0:
                        dto.Slot0Price = info.Price;
                        dto.StickerSlot1Name = info.Name;
                        break;
                    case 1:
                        dto.Slot1Price = info.Price;
                        dto.StickerSlot2Name = info.Name;
                        break;
                    case 2:
                        dto.Slot2Price = info.Price;
                        dto.StickerSlot3Name = info.Name;
                        break;
                    case 3:
                        dto.Slot3Price = info.Price;
                        dto.StickerSlot4Name = info.Name;
                        break;
                }
            }

            FillSlot(0, 0);
            FillSlot(1, 1);
            FillSlot(2, 2);
            FillSlot(3, 3);

            return dto;
        }
    }
}