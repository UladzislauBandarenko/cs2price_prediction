using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using cs2price_prediction.Data;

namespace cs2price_prediction.Services.Stickers
{
    public class StickerService : IStickerService
    {
        private readonly AppDbContext _db;

        public StickerService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<StickerFeatures> CalculateFeaturesAsync(IReadOnlyCollection<int> stickerIds)
        {
            var result = new StickerFeatures();

            if (stickerIds == null || stickerIds.Count == 0)
                return result;

            // Заглушка: если есть таблица sticker_prices, можно реально посчитать
            var prices = await _db.StickerPrices
                .Where(p => stickerIds.Contains(p.StickerId))
                .Select(p => p.Price)
                .ToListAsync();

            if (prices.Count == 0)
                return result;

            result.StickersCount = prices.Count;
            result.StickersTotalValue = prices.Sum();
            result.StickersAvgValue = prices.Average();
            result.StickersMaxValue = prices.Max();

            // Пока просто кладем те же значения во все слоты
            // TODO: если у тебя есть логика по slot0..slot3, подставь её
            result.Slot0Price = prices.ElementAtOrDefault(0);
            result.Slot1Price = prices.ElementAtOrDefault(1);
            result.Slot2Price = prices.ElementAtOrDefault(2);
            result.Slot3Price = prices.ElementAtOrDefault(3);

            return result;
        }
    }
}
