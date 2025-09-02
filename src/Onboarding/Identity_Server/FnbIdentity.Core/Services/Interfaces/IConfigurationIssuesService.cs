using FnbIdentity.Core.Dtos.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Services.Interfaces
{
    public interface IConfigurationIssuesService
    {
        Task<List<ConfigurationIssueDto>> GetAllIssuesAsync();
    }
}
