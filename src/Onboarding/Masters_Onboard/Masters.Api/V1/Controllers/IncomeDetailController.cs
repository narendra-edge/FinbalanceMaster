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
    public class IncomeDetailController : ControllerBase
    {
        private readonly IIncomeDetailService _incomeDetail;

        public IncomeDetailController(IIncomeDetailService incomeDetail)
        {
            _incomeDetail = incomeDetail ?? throw new ArgumentNullException(nameof(incomeDetail));
        }
        /// <summary>
        /// Get All The IncomeDetail.
        /// </summary>
        /// <returns>IncomeDetail</returns>
        /// <remarks>
        ///  - Table Uses => IncomeDetails
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetIncomeDetails")]
        // [SwaggerOperation("GetIncomeDetails")]      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<IncomeDetail>>> GetIncomeDetails()
        {
            Log.Information("Income Detail GetIncomeDetails triggred");
            var response = await _incomeDetail.GetAlIncomeDetail().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get The IncomeDetail  by Id.
        /// </summary>
        /// <param name="id">4</param>
        /// <returns>IncomeDetails</returns>
        /// <remarks>
        ///  - Table Uses => IncomeDetails        
        /// </remarks>
        [HttpGet("{id}", Name = "GetIncomeDetailById")]
        // [SwaggerOperation("GetIncomeDetailById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<IncomeDetail>>> GetIncomeDetailById(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var response = await _incomeDetail.GetIncomeDetailById(id).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
