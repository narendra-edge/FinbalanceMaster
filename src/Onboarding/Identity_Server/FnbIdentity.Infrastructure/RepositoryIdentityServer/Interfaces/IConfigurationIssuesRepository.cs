using FnbIdentity.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces
{
    public interface IConfigurationIssuesRepository
    {
        Task<List<ConfigurationIssueView>> GetClientIssuesAsync();
        Task<List<ConfigurationIssueView>> GetApiResourceIssuesAsync();
        Task<List<ConfigurationIssueView>> GetIdentityResourceIssuesAsync();
        Task<List<ConfigurationIssueView>> GetApiScopeIssuesAsync();
    }
}
