using FnbIdentity.Core.Dtos.IdentityProviders;
using FnbIdentity.Core.Services.Interfaces;
using FnbIdentity.UI.API.Configuration.Constants;
using FnbIdentity.UI.API.Dtos.IdentityProvider;
using FnbIdentity.UI.API.ExceptionHandeling;
using FnbIdentity.UI.API.Mappers;
using FnbIdentity.UI.API.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(ControllerExceptionFilterAttribute))]
    [Produces("application/json", "application/problem+json")]
    [Authorize(Policy = AuthorizationConsts.AdministrationPolicy)]
    public class IdentityProvidersController : ControllerBase
    {
        private readonly IIdentityProviderService _identityProviderService;
        private readonly IApiErrorResources _errorResources;

        public IdentityProvidersController(IIdentityProviderService identityProviderService, IApiErrorResources errorResources)
        {
            _identityProviderService = identityProviderService;
            _errorResources = errorResources;
        }

        [HttpGet]
        public async Task<ActionResult<IdentityProvidersApiDto>> Get(string searchText, int page = 1, int pageSize = 10)
        {
            var identityProvidersDto = await _identityProviderService.GetIdentityProvidersAsync(searchText, page, pageSize);
            var identityProvidersApiDto = identityProvidersDto.ToIdentityProviderApiModel<IdentityProvidersApiDto>();

            return Ok(identityProvidersApiDto);
        }

        [HttpGet(nameof(CanInsertIdentityProvider))]
        public async Task<ActionResult<bool>> CanInsertIdentityProvider(int id, string schema)
        {
            var exists = await _identityProviderService.CanInsertIdentityProviderAsync(new IdentityProviderDto
            {
                Id = id,
                Scheme = schema
            });

            return exists;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdentityProviderApiDto>> Get(int id)
        {
            var identityProviderDto = await _identityProviderService.GetIdentityProviderAsync(id);
            var identityProviderApiModel = identityProviderDto.ToIdentityProviderApiModel<IdentityProviderApiDto>();

            return Ok(identityProviderApiModel);
        }

        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Post([FromBody] IdentityProviderApiDto identityProviderApi)
        {
            var identityProviderDto = identityProviderApi.ToIdentityProviderApiModel<IdentityProviderDto>();

            if (!identityProviderDto.Id.Equals(default))
            {
                return BadRequest(_errorResources.CannotSetId());
            }

            var id = await _identityProviderService.AddIdentityProviderAsync(identityProviderDto);
            identityProviderApi.Id = id;

            return CreatedAtAction(nameof(Get), new { id }, identityProviderApi);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] IdentityProviderApiDto identityProviderApi)
        {
            var identityProvider = identityProviderApi.ToIdentityProviderApiModel<IdentityProviderDto>();

            await _identityProviderService.GetIdentityProviderAsync(identityProvider.Id);
            await _identityProviderService.UpdateIdentityProviderAsync(identityProvider);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var identityProvider = new IdentityProviderDto { Id = id };

            await _identityProviderService.GetIdentityProviderAsync(identityProvider.Id);
            await _identityProviderService.DeleteIdentityProviderAsync(identityProvider);

            return Ok();
        }

    }
}
