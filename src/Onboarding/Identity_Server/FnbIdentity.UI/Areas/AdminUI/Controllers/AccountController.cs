using FnbIdentity.UI.Configuration.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.Areas.AdminUI.Controllers
{
	[Authorize]
	[Area(CommonConsts.AdminUIArea)]
	public class AccountController : BaseController
	{		
		public AccountController(ILogger<ConfigurationController> logger) : base(logger)
		{
		}
		public IActionResult AccessDenied()
		{
			return View();
		}

		public IActionResult Logout()
		{
			return new SignOutResult(new List<string> { AuthenticationConsts.SignInScheme, AuthenticationConsts.OidcAuthenticationScheme });
		}
	}
}
