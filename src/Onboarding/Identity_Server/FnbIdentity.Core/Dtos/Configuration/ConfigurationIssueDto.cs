using FnbIdentity.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class ConfigurationIssueDto
    {
        public int ResourceId { get; set; }
        public string ResourceName { get; set; }
        public ConfigurationIssueMessageEnum Message { get; set; }
        public ConfigurationIssueTypeView IssueType { get; set; }
        public ConfigurationResourceType ResourceType { get; set; }
    }
}
