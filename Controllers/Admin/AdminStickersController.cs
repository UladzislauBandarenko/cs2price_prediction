using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Stickers;
using cs2price_prediction.Services.Admin.Stickers;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/stickers")]
    public class AdminStickersController : ControllerBase
    {
        private readonly IAdminStickerService _service;

        public AdminStickersController(IAdminStickerService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateSticker([FromBody] CreateStickerDto dto)
        {
            var id = await _service.CreateStickerAsync(dto);
            return CreatedAtAction(nameof(CreateSticker), new { id }, id);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSticker(int id, [FromBody] UpdateStickerDto dto)
        {
            var ok = await _service.UpdateStickerAsync(id, dto);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSticker(int id)
        {
            var ok = await _service.DeleteStickerAsync(id);
            if (!ok)
                return NotFound();

            return NoContent();
        }
    }
}
