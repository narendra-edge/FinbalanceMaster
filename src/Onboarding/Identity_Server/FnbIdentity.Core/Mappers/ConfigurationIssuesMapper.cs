using FnbIdentity.Core.Dtos.Configuration;
using FnbIdentity.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Mappers
{
    public static class ConfigurationIssuesMapper
    {
        public static ConfigurationIssueDto Map(this ConfigurationIssueView issueView, ConfigurationResourceType resourceType)
        {
            return new ConfigurationIssueDto
            {
                ResourceType = resourceType,
                ResourceId = issueView.ResourceId,
                IssueType = issueView.IssueType,
                Message = issueView.Message,
                ResourceName = issueView.ResourceName,
            };
        }
    }
}
