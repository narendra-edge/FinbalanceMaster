using Masters.Core.Interfaces.Services;
using Masters.Core.Models;
using Masters.Core.Services;
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
    public class PincodeMasterController : ControllerBase
    {
        private readonly IPincodeMasterService _pincodeMaster;

        public PincodeMasterController(IPincodeMasterService pincodeMaster)
        {
            _pincodeMaster = pincodeMaster ?? throw new ArgumentNullException(nameof(pincodeMaster));
        }
        /// <summary>
        /// Get All The PincodeMaster.
        /// </summary>
        /// <returns>PincodeMaster</returns>
        /// <remarks>
        ///  - Table Uses => PincodeMaster
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetPincodeMasters")]
        // [SwaggerOperation("PincodeMasters")]
        //[Route("getinstruments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<PincodeMaster>>> GetPincodeMasters()
        {
            Log.Information("PincodeMaster GetPincodeMasters triggred");
            var response = await _pincodeMaster.GetAllPinCodeMaster().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }
        /// <summary>
        /// Get The pincode by Id.
        /// </summary>
        /// <param name="PinId">4</param>
        /// <returns>PincodeMaster</returns>
        /// <remarks>
        ///  - Table Uses => pincodeMaster        
        /// </remarks>
        [HttpGet("{PinId}", Name = "GetpincodeMasterById")]
        // [SwaggerOperation("GetpincodeMasterById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<PincodeMaster>>> GetpincodeMasterById(int PinId)
        {
            if (PinId <= 0)
            {
                return NotFound();
            }
            var response = await _pincodeMaster.GetPinCodeMasterById(PinId).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
