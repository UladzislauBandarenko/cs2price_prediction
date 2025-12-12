// File: Services/Stickers/StickerService.cs
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

        /// <summary>
        /// Calculate sticker-related features.
        /// - Preserves input order and counts duplicate sticker IDs as separate slots.
        /// - Caller (PredictionService) enforces number-of-stickers limit.
        /// - If a sticker id is not present in DB, its price is treated as 0.
        /// </summary>
        public async Task<StickerFeatures> CalculateFeaturesAsync(IReadOnlyCollection<int> stickerIds)
        {
            var result = new StickerFeatures();

            if (stickerIds == null || stickerIds.Count == 0)
                return result;

            // Convert to list to preserve order and enable ElementAtOrDefault
            var ids = stickerIds.ToList();

            // Query DB once for unique ids -> price mapping
            var uniqueIds = ids.Distinct().ToList();

            // Fetch sticker prices for unique IDs and map them to a dictionary.
            // Assume StickerPrices has properties: StickerId (int) and Price (decimal).
            var priceDict = await _db.StickerPrices
                .Where(p => uniqueIds.Contains(p.StickerId))
                .ToDictionaryAsync(p => p.StickerId, p => p.Price);

            // Map each input id to its price (or 0 if not found). Preserve duplicates.
            // If StickerFeatures expects double, cast decimal -> double. If it uses decimal, remove cast.
            var prices = ids
                .Select(id =>
                {
                    if (priceDict.TryGetValue(id, out var price))
                    {
                        // Cast to double because StickerFeatures fields are expected to be double in this code.
                        return (double)price;
                    }
                    else
                    {
                        return 0.0;
                    }
                })
                .ToList();

            if (prices.Count == 0)
                return result;

            // Fill aggregated fields
            result.StickersCount = prices.Count;
            result.StickersTotalValue = prices.Sum();
            result.StickersAvgValue = prices.Average();
            result.StickersMaxValue = prices.Max();

            // Fill slot prices according to input order; missing slots default to 0
            result.Slot0Price = prices.ElementAtOrDefault(0);
            result.Slot1Price = prices.ElementAtOrDefault(1);
            result.Slot2Price = prices.ElementAtOrDefault(2);
            result.Slot3Price = prices.ElementAtOrDefault(3);

            return result;
        }
    }
}
