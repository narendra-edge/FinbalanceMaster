using Masters.Core.Interfaces.Services;
using Masters.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Masters.Api.V2.Controllers
{
    [Route("api/" + ApiConstants.ServiceName + "/v{api-version:apiversion}/[controller]")]
    [ApiVersion("2.0")]
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
        /// </remarks>
        [HttpGet]
        // [SwaggerOperation("GetInstruments")]
        //[Route("getinstruments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
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
    }
}
