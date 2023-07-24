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
    public class DistrictMasterController : ControllerBase
    {
        private readonly IDistrictMasterService _districtMaster;

        public DistrictMasterController(IDistrictMasterService districtMaster)
        {
            _districtMaster = districtMaster ?? throw new ArgumentNullException(nameof(districtMaster));
        }
        /// <summary>
        /// Get All The DistrictMaster.
        /// </summary>
        /// <returns>DistrictMaster</returns>
        /// <remarks>
        ///  - Table Uses => DistrictMaster
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetDistrictMasters")]
        // [SwaggerOperation("GetExchanges")]      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<DistrictMaster>>> GetDistrictMasters()
        {
            Log.Information("District Master DistrictMaster triggred");
            var response = await _districtMaster.GetAllDistrictMaster().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get The DistrictMaster by Id.
        /// </summary>
        /// <param name="DstrId">4</param>
        /// <returns>DistrictMaster</returns>
        /// <remarks>
        ///  - Table Uses => DistrictMaster        
        /// </remarks>
        [HttpGet("{DstrId}", Name = "GetDistrictMasterById")]
        // [SwaggerOperation("GetDistrictMasterById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<DistrictMaster>>> GetDistrictMasterById(int DstrId)
        {
            if (DstrId <= 0)
            {
                return NotFound();
            }
            var response = await _districtMaster.GetDistrictMasterById(DstrId).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
