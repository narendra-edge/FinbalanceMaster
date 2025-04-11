using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Common
{
    public class Pager
    {
        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public string? Action { get; set; }

        public string? Search { get; set; }

        public bool EnableSearch { get; set; } = false;

        public int MaxPages { get; set; } = 10;
    }
}
