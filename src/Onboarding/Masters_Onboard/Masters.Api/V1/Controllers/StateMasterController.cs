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
    public class StateMasterController : ControllerBase
    {
        private readonly IStateMasterService _stateMaster;

        public StateMasterController(IStateMasterService stateMaster)
        {
            _stateMaster = stateMaster ?? throw new ArgumentNullException(nameof(stateMaster));
        }
        /// <summary>
        /// Get All The StateMaster.
        /// </summary>
        /// <returns>StateMasters</returns>
        /// <remarks>
        ///  - Table Uses => StateMasters
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetStateMaster")]
        // [SwaggerOperation("GetStateMaster")]      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<StateMaster>>> GetStateMaster()
        {
            Log.Information("State Master  GetStateMasterById triggred");
            var response = await _stateMaster.GetAllStateMaster().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get The StateMaster by Id.
        /// </summary>
        /// <param name="StateId">4</param>
        /// <returns>StateMaster</returns>
        /// <remarks>
        ///  - Table Uses => GetStateMaster        
        /// </remarks>
        [HttpGet("{StateId}", Name = "GetStateMasterById")]
        // [SwaggerOperation("GetStateMasterById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<StateMaster>>> GetStateMasterById(int StateId)
        {
            if (StateId <= 0)
            {
                return NotFound();
            }
            var response = await _stateMaster.GetStateMasterById(StateId).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }
    }
}
