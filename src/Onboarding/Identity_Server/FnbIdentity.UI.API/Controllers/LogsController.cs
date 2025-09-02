using FnbIdentity.UI.API.Configuration.Constants;
using FnbIdentity.UI.API.ExceptionHandeling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
