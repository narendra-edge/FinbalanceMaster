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
    public class CountryCodeController : ControllerBase
    {
        private readonly ICountryCodeService _countryCode;

        public CountryCodeController(ICountryCodeService countryCode)
        {
            _countryCode = countryCode ?? throw new ArgumentNullException(nameof(countryCode));
        }
        /// <summary>
        /// Get All The CountryCode.
        /// </summary>
        /// <returns>CountryCodes</returns>
        /// <remarks>
        ///  - Table Uses => CountryCodes
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetCountryCodes")]
        // [SwaggerOperation("GetCountryCodes")]      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<CountryCode>>> GetCountryCodes()
        {
            Log.Information("Country Code GetCountryCodes triggred");
            var response = await _countryCode.GetAlCountryCode().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get The CountryCodes by Id.
        /// </summary>
        /// <param name="CtryId">4</param>
        /// <returns>CountryCodes</returns>
        /// <remarks>
        ///  - Table Uses => GetCountryCode        
        /// </remarks>
        [HttpGet("{CtryId}", Name = "GetCountryCodeById")]
        // [SwaggerOperation("GetCountryCodeById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<CountryCode>>> GetCountryCodeById(int CtryId)
        {
            if (CtryId <= 0)
            {
                return NotFound();
            }
            var response = await _countryCode.GetCountryCodeById(CtryId).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
