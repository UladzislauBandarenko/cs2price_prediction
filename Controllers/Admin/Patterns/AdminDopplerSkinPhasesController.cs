using System;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.DopplerSkin;
using cs2price_prediction.Services.Admin.Patterns.DopplerSkinPhase;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin.Patterns
{
    [ApiController]
    [Route("api/admin/patterns/doppler/skin-phases")]
    public class AdminDopplerSkinPhasesController : ControllerBase
    {
        private readonly IAdminDopplerSkinPhaseService _service;

        public AdminDopplerSkinPhasesController(IAdminDopplerSkinPhaseService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateDopplerSkinPhaseDto dto)
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDopplerSkinPhaseDto dto)
        {
            try
            {
                var ok = await _service.UpdateAsync(id, dto);
                if (!ok) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
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
