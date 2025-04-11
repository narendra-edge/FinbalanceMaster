using FnbIdentity.STS.Identity.Helpers;
using FnbIdentity.STS.Identity.ViewModels.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FnbIdentity.STS.Identity.Controllers
{
    [SecurityHeaders]
    [Authorize]
    public class DiagnosticsController : Controller
    {
        public async Task<IActionResult> Index()
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            var localAddresses = new string[] { "127.0.0.1", "::1", HttpContext.Connection?.LocalIpAddress?.ToString() };
#pragma warning restore CS8601 // Possible null reference assignment.
            if (!localAddresses.Contains(HttpContext.Connection?.RemoteIpAddress?.ToString()))
            {
                return NotFound();
            }

            var model = new DiagnosticsViewModel(await HttpContext.AuthenticateAsync());
            return View(model);
        }
    }
}
