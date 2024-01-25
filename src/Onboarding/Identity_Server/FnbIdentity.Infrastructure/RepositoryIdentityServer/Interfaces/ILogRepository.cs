using FnbIdentity.Infrastructure.Common;
using FnbIdentity.Infrastructure.Entities;

namespace FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces
{
    public interface ILogRepository
    {
        Task<PagedList<Log>> GetLogsAsync(string search, int page = 1, int pageSize = 10);

        Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan);

        bool AutoSaveChanges { get; set; }
    }
}
