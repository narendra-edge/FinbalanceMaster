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
    public class ExchangeController : ControllerBase
    {
        private readonly IExchangeService _exchangeService;
        
        public ExchangeController(IExchangeService exchangeService)
        {
            _exchangeService = exchangeService ?? throw  new ArgumentNullException(nameof(exchangeService));
            
        }
        /// <summary>
        /// Get All The Exchanges.
        /// </summary>
        /// <returns>Exchanges</returns>
        /// <remarks>
        ///  - Table Uses => Exchanges
        ///  -  ** Description **
        ///  - This endpoint will be used in Superadmin/admin UI Screen
        ///  - This endpoint will be  accesable only roles for superadmin,admin,developers etc...
        /// </remarks>
        [HttpGet(Name = "GetExchanges")]
        // [SwaggerOperation("GetExchanges")]      
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<Exchange>>> GetExchanges()
        {
            Log.Information("Exchange GetExchanges triggred");
            var response = await _exchangeService.GetAllExchanges().ConfigureAwait(false);
            if (response == null)
            {
                return NoContent();
            }
            return Ok(response);
        }

        /// <summary>
        /// Get The Exchange by Id.
        /// </summary>
        /// <param name="ExchId">4</param>
        /// <returns>Exchanges</returns>
        /// <remarks>
        ///  - Table Uses => Exchange        
        /// </remarks>
        [HttpGet("{ExchId}", Name = "GetExchangeById")]
        // [SwaggerOperation("GetExchangeById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<Exchange>>> GetExchangeById(int ExchId)
        {
            if (ExchId <= 0)
            {
                return NotFound();
            }
            var response = await _exchangeService.GetExchangeById(ExchId).ConfigureAwait(false);
            return response != null ? Ok(response) : NotFound();
        }

        /// <summary>
        /// Get Create Exchange.
        /// </summary>
        /// <returns>Exchanges</returns>
        /// <remarks>
        ///  - Table Uses => Exchange
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        [HttpPost]
        // [SwaggerOperation("CreateExchange")]       
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<IEnumerable<Exchange>>> CreateExchange(Exchange exchange)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var response = await _exchangeService.CreateExchange(exchange).ConfigureAwait(false);
            return CreatedAtRoute(nameof(GetExchangeById), new { ExchId = response.ExchId }, response);
        }
        /// <summary>
        /// Get Delete Exchange.
        /// </summary>
        /// <returns>true/false</returns>
        /// <remarks>
        ///  - Table Uses => Exchange
        /// </remarks>
        [HttpDelete("{ExchId}", Name = "DeleteExchange")]
        // [SwaggerOperation("DeleteExchange")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<bool>> DeleteExchange(int ExchId)
        {
            if (ExchId <= 0)
            {
                return BadRequest();
            }
            return await _exchangeService.DeleteExchange(ExchId).ConfigureAwait(false);
        }
        /// <summary>
        /// Get Update Exchanage .
        /// </summary>
        /// <returns>true/false</returns>
        /// <remarks>
        ///  - Table Uses => Exchanage
        /// </remarks>
        [HttpPut("{ExchId}", Name = "UpdateExchange")]
        // [SwaggerOperation("UpdateExchange")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<bool>> UpdateExchange(int ExchId, Exchange exchange)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (ExchId <= 0)
            {
                return BadRequest();
            }
            return await _exchangeService.UpdateExchange(ExchId, exchange).ConfigureAwait(false);

        }
    }
}
