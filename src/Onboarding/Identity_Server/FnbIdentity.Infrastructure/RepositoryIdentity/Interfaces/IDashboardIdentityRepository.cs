using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.RepositoryIdentity.Interfaces
{
    public interface IDashboardIdentityRepository
    {
        public Task<int> GetUsersTotalCountAsync(CancellationToken cancellationToken = default);

        public Task<int> GetRolesTotalCountAsync(CancellationToken cancellationToken = default);
    }
}
