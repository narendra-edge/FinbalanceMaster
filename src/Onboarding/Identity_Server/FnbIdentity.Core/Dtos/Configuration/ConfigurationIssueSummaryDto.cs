using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class ConfigurationIssueSummaryDto
    {
        public int Warnings { get; set; }
        public int Recommendations { get; set; }
    }
}
