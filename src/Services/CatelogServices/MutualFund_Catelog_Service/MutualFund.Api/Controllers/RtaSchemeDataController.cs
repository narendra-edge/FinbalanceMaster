using Microsoft.AspNetCore.Mvc;
using MutualFund.Application.DTOS;
using MutualFund.Application.Interfaces.Services;

namespace MutualFund.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RtaSchemeDataController : ControllerBase
    {
        private readonly IRtaSchemeDataService _rtaSchemeData;

        public RtaSchemeDataController(IRtaSchemeDataService rtaSchemeData)
        {
            _rtaSchemeData = rtaSchemeData;
        }

        // GET: api/RtaSchemeData
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RtaSchemeDataDto>>> GetAllAsync()
        {
            var response = await _rtaSchemeData.GetAllAsync().ConfigureAwait(false);
            if (response == null || !response.Any()) return NoContent();
            return Ok(response);
        }

        // GET: api/RtaSchemeData/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RtaSchemeDataDto?>> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid id.");
            var response = await _rtaSchemeData.GetByIdAsync(id).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }

        // GET: api/RtaSchemeData/bycode/{rtaSchemeCode}
        //[HttpGet("bycode/{rtaSchemeCode}")]
        //public async Task<ActionResult<RtaSchemeDataDto?>> GetByRtaSchemeCodeAsync(string rtaSchemeCode)
        //{
        //    if (string.IsNullOrWhiteSpace(rtaSchemeCode)) return BadRequest();
        //    var response = await _rtaSchemeData.GetByRtaSchemeCodeAsync(rtaSchemeCode).ConfigureAwait(false);
        //    return response != null ? Ok(response) : NotFound();
        //}

        // POST: api/RtaSchemeData
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] RtaSchemeDataDto dto)
        {
            if (dto == null) return BadRequest();
            await _rtaSchemeData.AddAsync(dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = dto.Id }, dto);
        }

        // PUT: api/RtaSchemeData
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] RtaSchemeDataDto dto)
        {
            if (dto == null || dto.Id == Guid.Empty) return BadRequest();
            await _rtaSchemeData.UpdateAsync(dto).ConfigureAwait(false);
            return NoContent();
        }

        // DELETE: api/RtaSchemeData/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid id.");
            await _rtaSchemeData.DeleteAsync(id).ConfigureAwait(false);
            return NoContent();
        }
    }
}
