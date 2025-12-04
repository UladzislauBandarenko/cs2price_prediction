using System;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Weapons;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Admin.Weapons;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/weapons")]
    public class AdminWeaponsController : ControllerBase
    {
        private readonly IAdminWeaponService _service;

        public AdminWeaponsController(IAdminWeaponService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<WeaponDto>> Create([FromBody] CreateWeaponDto dto)
        {
            try
            {
                var created = await _service.CreateWeaponAsync(dto);
                return CreatedAtAction(nameof(Create), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<WeaponDto>> Update(int id, [FromBody] UpdateWeaponDto dto)
        {
            try
            {
                var updated = await _service.UpdateWeaponAsync(id, dto);
                if (updated is null)
                    return NotFound();

                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteWeaponAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
