using FnbIdentity.Core.Dtos.Configuration;
using FnbIdentity.Infrastructure.Entities;


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
