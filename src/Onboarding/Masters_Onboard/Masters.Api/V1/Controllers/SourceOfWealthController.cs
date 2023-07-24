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
    public class SourceOfWealthController : ControllerBase
    {
        private readonly ISourceOfWealthService _sourceOfWealth;

        public SourceOfWealthController(ISourceOfWealthService sourceOfWealth)
        {
            _sourceOfWealth = sourceOfWealth ?? throw new ArgumentNullException(nameof(sourceOfWealth));
        }
        /// <summary>
        /// Get All The Source of Wealth.
        /// </summary>
        /// <returns>SourceOfWealth</returns>
        /// <remarks>
        ///  - Table Uses => SourceOfWealths
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetSourceOfWealths")]
        // [SwaggerOperation("GetSourceOfWealths")]      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<SourceOfWealth>>> GetSourceOfWealths()
        {
            Log.Information("Source of Wealth GetSourceOfWealths triggred");
            var response = await _sourceOfWealth.GetAlSourceOfWealth().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get The SourceOfWealth by Id.
        /// </summary>
        /// <param name="SrcId">4</param>
        /// <returns>SourceOfWealths</returns>
        /// <remarks>
        ///  - Table Uses => SourceOfWealths        
        /// </remarks>
        [HttpGet("{SrcId}", Name = "GetSourceOfWealthById")]
        // [SwaggerOperation("GetSourceOfWealthById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<SourceOfWealth>>> GetSourceOfWealthById(int SrcId)
        {
            if (SrcId <= 0)
            {
                return NotFound();
            }
            var response = await _sourceOfWealth.GetSourceOfWealthById(SrcId).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
