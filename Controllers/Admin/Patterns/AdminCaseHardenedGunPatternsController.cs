using System;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedGun;
using cs2price_prediction.Services.Admin.Patterns.CaseHardenedGun;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin.Patterns
{
    [ApiController]
    [Route("api/admin/patterns/case-hardened/gun")]
    public class AdminCaseHardenedGunPatternsController : ControllerBase
    {
        private readonly IAdminCaseHardenedGunPatternService _service;

        public AdminCaseHardenedGunPatternsController(IAdminCaseHardenedGunPatternService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateCaseHardenedGunPatternDto dto)
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCaseHardenedGunPatternDto dto)
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
