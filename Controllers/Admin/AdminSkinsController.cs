using System;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Skins;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Admin.Skins;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/skins")]
    public class AdminSkinsController : ControllerBase
    {
        private readonly IAdminSkinService _service;

        public AdminSkinsController(IAdminSkinService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<SkinDto>> Create([FromBody] CreateSkinDto dto)
        {
            try
            {
                var created = await _service.CreateSkinAsync(dto);
                return CreatedAtAction(nameof(Create), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<SkinDto>> Update(int id, [FromBody] UpdateSkinDto dto)
        {
            try
            {
                var updated = await _service.UpdateSkinAsync(id, dto);
                if (updated is null)
                    return NotFound();

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteSkinAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
