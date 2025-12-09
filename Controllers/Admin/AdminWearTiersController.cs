using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.WearTiers;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Admin.WearTiers;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/wear-tiers")]
    public class AdminWearTiersController : ControllerBase
    {
        private readonly IAdminWearTierService _service;

        public AdminWearTiersController(IAdminWearTierService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<WearTierDto>> Create([FromBody] CreateWearTierDto dto)
        {
            var created = await _service.CreateWearTierAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<WearTierDto>> Update(int id, [FromBody] UpdateWearTierDto dto)
        {
            var updated = await _service.UpdateWearTierAsync(id, dto);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteWearTierAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
