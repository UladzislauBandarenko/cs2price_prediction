using System;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.FadeKnife;
using cs2price_prediction.Services.Admin.Patterns.FadeKnife;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin.Patterns
{
    [ApiController]
    [Route("api/admin/patterns/fade/knife")]
    public class AdminFadeKnifePatternsController : ControllerBase
    {
        private readonly IAdminFadeKnifePatternService _service;

        public AdminFadeKnifePatternsController(IAdminFadeKnifePatternService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateFadeKnifePatternDto dto)
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFadeKnifePatternDto dto)
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
