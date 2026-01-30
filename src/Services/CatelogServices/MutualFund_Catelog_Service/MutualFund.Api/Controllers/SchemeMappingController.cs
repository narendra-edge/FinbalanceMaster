using Microsoft.AspNetCore.Mvc;
using MutualFund.Application.DTOS;
using MutualFund.Application.Interfaces.Services;

namespace MutualFund.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchemeMappingController : ControllerBase
    {
        private readonly ISchemeMappingService _service;

        public SchemeMappingController(ISchemeMappingService service)
        {
            _service = service;
        }

        // GET: api/SchemeMapping
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SchemeMappingDto>>> GetAllAsync()
        {
            var list = await _service.GetAllMappingsAsync().ConfigureAwait(false);
            if (list == null || !list.Any()) return NoContent();
            return Ok(list);
        }

        // GET: api/SchemeMapping/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<SchemeMappingDto?>> GetByIdAsync(long id)
        {
            if (id <= 0) return BadRequest("Invalid id.");
            var dto = await _service.GetMappingByIdAsync(id).ConfigureAwait(false);
            return dto == null ? NotFound() : Ok(dto);
        }

        // POST: api/SchemeMapping
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] SchemeMappingDto dto)
        {
            if (dto == null) return BadRequest();
            await _service.AddMappingAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = dto.Id }, dto);
        }

        // PUT: api/SchemeMapping
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] SchemeMappingDto dto)
        {
            if (dto == null) return BadRequest();
            await _service.UpdateMappingAsync(dto).ConfigureAwait(false);
            return NoContent();
        }

        // DELETE: api/SchemeMapping/{id}
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            if (id <= 0) return BadRequest("Invalid id.");
            await _service.DeleteMappingAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
