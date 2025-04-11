using FnbIdentity.Core.Dtos.Logs;
using FnbIdentity.Core.Services.Interfaces;
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
	[Authorize(Policy = AuthorizationConsts.AdministrationPolicy)]
	[Area(CommonConsts.AdminUIArea)]
	public class LogController : BaseController
	{
		private readonly ILogService _logService;

		public LogController(ILogService logService,
			ILogger<ConfigurationController> logger) : base(logger)
		{
			_logService = logService;
			
		}
		[HttpGet]
		public async Task<IActionResult> ErrorsLog(int? page, string search)
		{
			ViewBag.Search = search;
			var logs = await _logService.GetLogsAsync(search, page ?? 1);

			return View(logs);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteLogs(LogsDto log)
		{
			if (!ModelState.IsValid)
			{
				return View(nameof(ErrorsLog), log);
			}

			await _logService.DeleteLogsOlderThanAsync(log.DeleteOlderThan.Value);

			return RedirectToAction(nameof(ErrorsLog));
		}

		
	}
}
