using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Skins;
using cs2price_prediction.DTOs.Meta;

namespace cs2price_prediction.Services.Admin.Skins
{
    public interface IAdminSkinService
    {
        Task<SkinDto> CreateSkinAsync(CreateSkinDto dto);
        Task<SkinDto?> UpdateSkinAsync(int id, UpdateSkinDto dto);
        Task<bool> DeleteSkinAsync(int id);
    }
}
