using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.Helpers.Localization
{
	public interface IGenericControllerLocalizer<out T>
	{
		LocalizedString this[string name] { get; }

		LocalizedString this[string name, params object[] arguments] { get; }

		IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures);
	}
}
