using Microsoft.AspNetCore.Mvc;
using MutualFund.Application.DTOS;
using MutualFund.Application.Interfaces.Services;

namespace MutualFund.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmfiSchemeController : ControllerBase
    {
        private readonly IAmfiService _amfiService;

        public AmfiSchemeController(IAmfiService amfiService)
        {
            _amfiService = amfiService;
        }

        // GET: api/AmfiScheme
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AmfiSchemeDto>>> GetAllSchemesAsync()
        {
            var response = await _amfiService.GetAllSchemesAsync().ConfigureAwait(false);
            if (response == null || !response.Any()) return NoContent();
            return Ok(response);                        
        }

        // GET: api/AmfiScheme/123
        [HttpGet("{code:int}")]
        public async Task<ActionResult<AmfiSchemeDto?>> GetByCodeAsync(int code)
        {
            if (code <= 0) return BadRequest("Invalid code.");

            var response = await _amfiService.GetByCodeAsync(code).ConfigureAwait(false);
            if (response == null) return NotFound();
            return Ok(response);
        }
    }
}
