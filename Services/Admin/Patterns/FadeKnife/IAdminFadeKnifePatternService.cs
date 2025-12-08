using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.FadeKnife;

namespace cs2price_prediction.Services.Admin.Patterns.FadeKnife
{
    public interface IAdminFadeKnifePatternService
    {
        Task<int> CreateAsync(CreateFadeKnifePatternDto dto);
        Task<bool> UpdateAsync(int id, UpdateFadeKnifePatternDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
