using Duende.IdentityServer.EntityFramework.Entities;
using FnbIdentity.Infrastructure.Common;
using FnbIdentity.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.RepositoryIdentity.Interfaces
{
    public interface IPersistedGrantAspNetIdentityRepository
    {
        Task<PagedList<PersistedGrantDataView>> GetPersistedGrantsByUsersAsync(string search, int page = 1, int pageSize = 10);
        Task<PagedList<PersistedGrant>> GetPersistedGrantsByUserAsync(string subjectId, int page = 1, int pageSize = 10);
        Task<PersistedGrant> GetPersistedGrantAsync(string key);
        Task<int> DeletePersistedGrantAsync(string key);
        Task<int> DeletePersistedGrantsAsync(string userId);
        Task<bool> ExistsPersistedGrantsAsync(string subjectId);
        Task<bool> ExistsPersistedGrantAsync(string key);
        Task<int> SaveAllChangesAsync();
        bool AutoSaveChanges { get; set; }
    }
}
