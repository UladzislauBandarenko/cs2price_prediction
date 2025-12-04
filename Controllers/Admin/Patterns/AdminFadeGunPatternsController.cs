using System;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.FadeGun;
using cs2price_prediction.Services.Admin.Patterns.FadeGun;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin.Patterns
{
    [ApiController]
    [Route("api/admin/patterns/fade/gun")]
    public class AdminFadeGunPatternsController : ControllerBase
    {
        private readonly IAdminFadeGunPatternService _service;

        public AdminFadeGunPatternsController(IAdminFadeGunPatternService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateFadeGunPatternDto dto)
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFadeGunPatternDto dto)
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
