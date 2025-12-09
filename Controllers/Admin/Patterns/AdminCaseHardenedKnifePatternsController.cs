using System;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedKnife;
using cs2price_prediction.Services.Admin.Patterns.CaseHardenedKnife;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin.Patterns
{
    [ApiController]
    [Route("api/admin/patterns/case-hardened/knife")]
    public class AdminCaseHardenedKnifePatternsController : ControllerBase
    {
        private readonly IAdminCaseHardenedKnifePatternService _service;

        public AdminCaseHardenedKnifePatternsController(IAdminCaseHardenedKnifePatternService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateCaseHardenedKnifePatternDto dto)
        {
            try
            {
                var id = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(Create), new { id }, id);
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCaseHardenedKnifePatternDto dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
