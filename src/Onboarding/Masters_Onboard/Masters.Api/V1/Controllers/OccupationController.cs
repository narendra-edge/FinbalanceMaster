using Masters.Core.Interfaces.Services;
using Masters.Core.Models;
using Masters.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog;
using static System.Net.Mime.MediaTypeNames;

namespace Masters.Api.V1.Controllers
{
    [Produces(Application.Json)]
    [Route("api/" + ApiConstants.ServiceName + "/v{api-version:apiversion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class OccupationController : ControllerBase
    {
        private readonly IOccupationService _occupation;

        public OccupationController(IOccupationService occupation)
        {
            _occupation = occupation ?? throw new ArgumentNullException(nameof(occupation));
        }
        /// <summary>
        /// Get All The Occupations.
        /// </summary>
        /// <returns>Occupations</returns>
        /// <remarks>
        ///  - Table Uses => Occupations
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetOccupations")]
        // [SwaggerOperation("GetOccupations")]      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<Occupation>>> GetOccupations()
        {
            Log.Information("Exchange GetExchanges triggred");
            var response = await _occupation.GetAlOccupation().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get The Occupation by Id.
        /// </summary>
        /// <param name="id">4</param>
        /// <returns>Occupation</returns>
        /// <remarks>
        ///  - Table Uses => Occupation       
        /// </remarks>
        [HttpGet("{id}", Name = "GetOccupationById")]
        // [SwaggerOperation("GetOccupationById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<Occupation>>> GetOccupationById(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var response = await _occupation.GetOccupationById(id).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
