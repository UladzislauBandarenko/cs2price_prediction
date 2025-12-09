using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.Doppler;
using cs2price_prediction.Services.Admin.Patterns.DopplerPhase;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin.Patterns
{
    [ApiController]
    [Route("api/admin/patterns/doppler/phases")]
    public class AdminDopplerPhasesController : ControllerBase
    {
        private readonly IAdminDopplerPhaseService _service;

        public AdminDopplerPhasesController(IAdminDopplerPhaseService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateDopplerPhaseDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Create), new { id }, id);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDopplerPhaseDto dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return Conflict("Phase not found or used by skins.");
            return NoContent();
        }
    }
}
