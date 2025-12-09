using System;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.SkinWearTiers;
using cs2price_prediction.Services.Admin.SkinWearTiers;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/skin-wear-tiers")]
    public class AdminSkinWearTiersController : ControllerBase
    {
        private readonly IAdminSkinWearTierService _service;

        public AdminSkinWearTiersController(IAdminSkinWearTierService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSkinWearTierDto dto)
        {
            try
            {
                var created = await _service.CreateSkinWearTierAsync(dto);
                if (!created)
                    return Conflict("SkinWearTier already exists.");

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSkinWearTierDto dto)
        {
            try
            {
                var ok = await _service.UpdateSkinWearTierAsync(dto);
                if (!ok)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] DeleteSkinWearTierDto dto)
        {
            var ok = await _service.DeleteSkinWearTierAsync(dto.SkinId, dto.WearTierId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
