using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MutualFund.Api.Models;

namespace MutualFund.Api.Controllers.V1
{
    [Route("api/SchemeApi")]
    [ApiController]
   // [Authorize]
    public class SchemeController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Scheme> GetSchemes()

        {
            return new List<Scheme> {
                new Scheme { SchemeId = 1,SchemeName ="Hadfc Tax Saver" },
                new Scheme { SchemeId = 2,SchemeName="ICICI Long Term Plan"}
            };
        }
    }
}
