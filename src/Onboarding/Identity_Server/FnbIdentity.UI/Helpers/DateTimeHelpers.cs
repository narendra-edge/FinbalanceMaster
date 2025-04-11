using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.Helpers
{
	public static class DateTimeHelpers
	{
		private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static double GetEpochTicks(this DateTimeOffset dateTime)
		{
			return dateTime.Subtract(Epoch).TotalMilliseconds;
		}
	}
}
