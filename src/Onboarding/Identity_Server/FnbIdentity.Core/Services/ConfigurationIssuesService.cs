using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FnbIdentity.Core.Dtos.Configuration;
using FnbIdentity.Core.Mappers;
using FnbIdentity.Core.Services.Interfaces;
using FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces;

namespace FnbIdentity.Core.Services
{
    public class ConfigurationIssuesService(IConfigurationIssuesRepository repository) : IConfigurationIssuesService
    {
        public async Task<List<ConfigurationIssueDto>> GetAllIssuesAsync()
        {
            var configurationIssues = new List<ConfigurationIssueDto>();

            var clientIssues = await repository.GetClientIssuesAsync();
            var clientConfigurationIssues = clientIssues.Select(x => x.Map(ConfigurationResourceType.Client)).ToList();

            configurationIssues.AddRange(clientConfigurationIssues);

            return configurationIssues;


        }
    }
}
