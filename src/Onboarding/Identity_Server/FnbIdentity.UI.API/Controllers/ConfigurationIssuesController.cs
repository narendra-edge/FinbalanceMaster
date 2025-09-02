using FnbIdentity.Core.Dtos.Configuration;
using FnbIdentity.Core.Services.Interfaces;
using FnbIdentity.Infrastructure.Entities;
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
    [Authorize(Policy = AuthorizationConsts.AdministrationPolicy)]
    [TypeFilter(typeof(ControllerExceptionFilterAttribute))]
    public class ConfigurationIssuesController(IConfigurationIssuesService configurationIssuesService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ConfigurationIssueDto>>> Get()
        {
            var issues = await configurationIssuesService.GetAllIssuesAsync();

            return Ok(issues);
        }

        [HttpGet(nameof(GetSummary))]
        public async Task<ActionResult<ConfigurationIssueSummaryDto>> GetSummary()
        {
            var issues = await configurationIssuesService.GetAllIssuesAsync();

            var summary = new ConfigurationIssueSummaryDto
            {
                Warnings = issues.Count(i => i.IssueType == ConfigurationIssueTypeView.Warning),
                Recommendations = issues.Count(i => i.IssueType == ConfigurationIssueTypeView.Recommendation)
            };

            return Ok(summary);
        }
    }
}
