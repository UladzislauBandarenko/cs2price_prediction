using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.WeaponTypes;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Admin.WeaponTypes;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/weapon-types")]
    public class AdminWeaponTypesController : ControllerBase
    {
        private readonly IAdminWeaponTypeService _service;

        public AdminWeaponTypesController(IAdminWeaponTypeService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<WeaponTypeDto>> Create([FromBody] CreateWeaponTypeDto dto)
        {
            var created = await _service.CreateWeaponTypeAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<WeaponTypeDto>> Update(int id, [FromBody] UpdateWeaponTypeDto dto)
        {
            var updated = await _service.UpdateWeaponTypeAsync(id, dto);
            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteWeaponTypeAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
