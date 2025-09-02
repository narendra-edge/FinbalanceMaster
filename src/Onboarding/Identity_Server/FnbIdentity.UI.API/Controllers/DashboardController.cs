using FnbIdentity.Core.Dtos.Dashboard;
using FnbIdentity.Core.IdentityDto.DashboardIdentity;
using FnbIdentity.Core.IdentityServices.Interfaces;
using FnbIdentity.Core.Services.Interfaces;
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
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IDashboardIdentityService _dashboardIdentityService;

        public  DashboardController(IDashboardService dashboardService, IDashboardIdentityService dashboardIdentityService)
        {
            _dashboardService = dashboardService;
            _dashboardIdentityService = dashboardIdentityService;
        }

        //[HttpGet(nameof(GetDashboardIdentityServer))]
        //public async Task<ActionResult<DashboardDto>> GetDashboardIdentityServer(int auditLogsLastNumberOfDays = 7)
        //{
        //    var dashboardIdentityServer = await _dashboardService.GetDashboardIdentityServerAsync(auditLogsLastNumberOfDays);

        //    return Ok(dashboardIdentityServer);
        //}

        [HttpGet(nameof(GetDashboardIdentity))]
        public async Task<ActionResult<DashboardIdentityDto>> GetDashboardIdentity()
        {
            var dashboardIdentity = await _dashboardIdentityService.GetIdentityDashboardAsync();

            return Ok(dashboardIdentity);
        }

    }
}
