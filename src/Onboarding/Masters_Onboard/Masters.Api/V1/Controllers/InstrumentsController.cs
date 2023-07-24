using Masters.Core.Interfaces.Services;
using Masters.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static System.Net.Mime.MediaTypeNames;

namespace Masters.Api.V1.Controllers
{
    [Produces(Application.Json)]
    [Route("api/" + ApiConstants.ServiceName + "/v{api-version:apiversion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class InstrumentsController : ControllerBase
    {
        private readonly IInstrumentService _instrumentservice;
        public InstrumentsController(IInstrumentService instrumentservice)
        {
            _instrumentservice = instrumentservice ?? throw new ArgumentNullException(nameof(instrumentservice));
        }
        /// <summary>
        /// Get All The Instruments.
        /// </summary>
        /// <returns>Instruments</returns>
        /// <remarks>
        ///  - Table Uses => Instrument
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetInstruments")]
        // [SwaggerOperation("GetInstruments")]
        //[Route("getinstruments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<Instrument>>> GetInstruments()
        {
            Log.Information("Instrument GetInstrument triggred");
            var response = await _instrumentservice.GetAllInstruments().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }
        /// <summary>
        /// Get The Instrument by Id.
        /// </summary>
        /// <param name="InstId">4</param>
        /// <returns>Instruments</returns>
        /// <remarks>
        ///  - Table Uses => Instrument        
        /// </remarks>
        [HttpGet("{InstId}", Name = "GetInstrumentById")]
        // [SwaggerOperation("GetInstruments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<Instrument>>> GetInstrumentById(int InstId)
        {
            if (InstId <= 0)
            {
                return NotFound();
            }
            var response = await _instrumentservice.GetInstrumentById(InstId).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
        /// <summary>
        /// Get Create Instrument.
        /// </summary>
        /// <returns>Instruments</returns>
        /// <remarks>
        ///  - Table Uses => Instrument
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        [HttpPost]
        // [SwaggerOperation("CreateInstrument")]       
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<Instrument>>> CreateInstument(Instrument instrument)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var response = await _instrumentservice.CreateInstrument(instrument).ConfigureAwait(false);
            return CreatedAtRoute(nameof(GetInstrumentById), new { InstId = response.InstId }, response);
        }

        /// <summary>
        /// Get Delete Instrument.
        /// </summary>
        /// <returns>true/false</returns>
        /// <remarks>
        ///  - Table Uses => Instrument
        /// </remarks>
        [HttpDelete("{InstId}", Name = "DeleteInstrument")]
        // [SwaggerOperation("GetInstruments")]
        //[Route("getinstruments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<bool>> DeleteInstrument(int InstId)
        {
            if (InstId <= 0)
            {
                return BadRequest();
            }
            return await _instrumentservice.DeleteInstrument(InstId).ConfigureAwait(false);
        }
        /// <summary>
        /// Get Update Instrument .
        /// </summary>
        /// <returns>true/false</returns>
        /// <remarks>
        ///  - Table Uses => Instrument
        /// </remarks>
        [HttpPut("{InstId}", Name = "UpdateInstrument")]
        // [SwaggerOperation("GetInstruments")]
        //[Route("getinstruments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<bool>> UpdateInstrument(int InstId, Instrument instrument)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (InstId <= 0)
            {
                return BadRequest();
            }
            return await _instrumentservice.UpdateInstrument(InstId, instrument).ConfigureAwait(false);
           
        }
    }
}
