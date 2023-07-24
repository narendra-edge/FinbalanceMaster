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
    public class TaxStauts : ControllerBase
    {
        private readonly ITaxStatusSerice _taxStatus;

        public TaxStauts(ITaxStatusSerice taxStatus)
        {
            _taxStatus = taxStatus ?? throw new ArgumentNullException(nameof(taxStatus));
        }
        /// <summary>
        /// Get All The TaxStatus.
        /// </summary>
        /// <returns>TaxStatus</returns>
        /// <remarks>
        ///  - Table Uses => TaxStatues
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetTaxStatues")]
        // [SwaggerOperation("GetTaxStatues")]      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<TaxStatus>>> GetTaxStatues()
        {
            Log.Information("Tax Stauts GetTaxStatues triggred");
            var response = await _taxStatus.GetAlTaxStatus().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get The TaxStatus by Id.
        /// </summary>
        /// <param name="id">4</param>
        /// <returns>TaxStatusById</returns>
        /// <remarks>
        ///  - Table Uses => TaxStatus       
        /// </remarks>
        [HttpGet("{id}", Name = "GetTaxStatusById")]
        // [SwaggerOperation("GetTaxStatusById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<TaxStatus>>> GetTaxStatusById(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var response = await _taxStatus.GetTaxStatusById(id).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
