using Microsoft.AspNetCore.Mvc;
using MutualFund.Application.DTOS;
using MutualFund.Application.Interfaces.Services;

namespace MutualFund.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchemeMasterFinalController : ControllerBase
    {
        private readonly ISchemeMasterFinalService _service;

        public SchemeMasterFinalController(ISchemeMasterFinalService service)
        {
            _service = service;
        }

        // GET: api/SchemeMasterFinal
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync().ConfigureAwait(false);
            if (list == null || !list.Any()) return NoContent();
            return Ok(list);
        }

        // GET: api/SchemeMasterFinal/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid id.");
            var dto = await _service.GetByIdAsync(id).ConfigureAwait(false);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // POST: api/SchemeMasterFinal
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SchemeMasterFinalDto dto)
        {
            if (dto == null) return BadRequest();
            await _service.AddAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        // PUT: api/SchemeMasterFinal
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SchemeMasterFinalDto dto)
        {
            if (dto == null) return BadRequest();
            await _service.UpdateAsync(dto).ConfigureAwait(false);
            return NoContent();
        }

        // DELETE: api/SchemeMasterFinal/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid id.");
            await _service.DeleteAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
