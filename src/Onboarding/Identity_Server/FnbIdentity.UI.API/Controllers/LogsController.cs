using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FnbIdentity.UI.API.Configuration.Constants;
using FnbIdentity.UI.API.ExceptionHandeling;
namespace FnbIdentity.UI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(ControllerExceptionFilterAttribute))]
    [Produces("application/json")]
    [Authorize(Policy = AuthorizationConsts.AdministrationPolicy)]
    public class LogsController : ControllerBase
    {
        //[HttpGet(nameof(AuditLog))]
        //public async Task<ActionResult<AuditLogsDto>> AuditLog([FromQuery] AuditLogFilterDto filters)
        //{
        //    var logs = await auditLogService.GetAsync(filters);

        //    return Ok(logs);
        //}
    }
}
