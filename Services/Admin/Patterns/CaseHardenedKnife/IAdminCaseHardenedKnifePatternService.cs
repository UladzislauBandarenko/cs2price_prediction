using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedKnife;

namespace cs2price_prediction.Services.Admin.Patterns.CaseHardenedKnife
{
    public interface IAdminCaseHardenedKnifePatternService
    {
        Task<int> CreateAsync(CreateCaseHardenedKnifePatternDto dto);
        Task<bool> UpdateAsync(int id, UpdateCaseHardenedKnifePatternDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
