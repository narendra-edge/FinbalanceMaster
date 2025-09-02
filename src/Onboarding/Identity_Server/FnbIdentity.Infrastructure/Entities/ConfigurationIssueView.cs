using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.Entities
{
    public class ConfigurationIssueView
    {
        public int ResourceId { get; set; }
        public string ResourceName { get; set; }
        public ConfigurationIssueMessageEnum Message { get; set; }
        public ConfigurationIssueTypeView IssueType { get; set; }
    }
}
