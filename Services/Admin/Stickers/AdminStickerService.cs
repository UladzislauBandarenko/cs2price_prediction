using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Stickers;
using cs2price_prediction.DTOs.Admin.Stickers;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.Stickers
{
    public class AdminStickerService : IAdminStickerService
    {
        private readonly IAdminDbContextFactory _factory;

        public AdminStickerService(IAdminDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<int> CreateStickerAsync(CreateStickerDto dto)
        {
            await using var db = _factory.CreateAdminContext();

            var sticker = new Sticker
            {
                Name = dto.Name.Trim()
            };

            db.Stickers.Add(sticker);

            if (dto.ReferencePrice.HasValue)
            {
                db.StickerPrices.Add(new StickerPrice
                {
                    Sticker = sticker,
                    Price = dto.ReferencePrice.Value
                });
            }

            await db.SaveChangesAsync();
            return sticker.Id;
        }

        public async Task<bool> UpdateStickerAsync(int id, UpdateStickerDto dto)
        {
            await using var db = _factory.CreateAdminContext();

            var sticker = await db.Stickers
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sticker == null)
                return false;

            sticker.Name = dto.Name.Trim();

            if (dto.ReferencePrice.HasValue)
            {
                db.StickerPrices.Add(new StickerPrice
                {
                    StickerId = sticker.Id,
                    Price = dto.ReferencePrice.Value
                });
            }

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStickerAsync(int id)
        {
            await using var db = _factory.CreateAdminContext();

            var sticker = await db.Stickers
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sticker == null)
                return false;

            db.Stickers.Remove(sticker);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
