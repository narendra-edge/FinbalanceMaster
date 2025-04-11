using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Shared.Configuration.Email
{
    public class SendGridConfiguration
    {
        public string ApiKey { get; set; }
        public string SourceEmail { get; set; }
        public string SourceName { get; set; }
        public bool EnableClickTracking { get; set; } = false;
    }
}
