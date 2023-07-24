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
    public class UBOCodeController : ControllerBase
    {
        private readonly IUBOService _uBOCode;

        public UBOCodeController(IUBOService uBOCode)
        {
            _uBOCode = uBOCode ?? throw new ArgumentNullException(nameof(uBOCode));
        }
        /// <summary>
        /// Get All The UBOCode.
        /// </summary>
        /// <returns>UBOCodes</returns>
        /// <remarks>
        ///  - Table Uses => UBOCodes
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetUBOCodes")]
        // [SwaggerOperation("GetUBOS")]      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<UBOCode>>> GetUBOCodes()
        {
            Log.Information("UBOCodes GetUBOCodes triggred");
            var response = await _uBOCode.GetAlUBOCode().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get The UBOCodes by Id.
        /// </summary>
        /// <param name="id">4</param>
        /// <returns>UBOCodes</returns>
        /// <remarks>
        ///  - Table Uses => UBOCodes        
        /// </remarks>
        [HttpGet("{id}", Name = "GetUBOCodesById")]
        // [SwaggerOperation("GetUBOCodesById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<UBOCode>>> GetUBOCodesById(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var response = await _uBOCode.GetUBOCodeById(id).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
