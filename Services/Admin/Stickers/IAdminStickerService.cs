using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Stickers;

namespace cs2price_prediction.Services.Admin.Stickers
{
    public interface IAdminStickerService
    {
        Task<int> CreateStickerAsync(CreateStickerDto dto);
        Task<bool> UpdateStickerAsync(int id, UpdateStickerDto dto);
        Task<bool> DeleteStickerAsync(int id);
    }
}
