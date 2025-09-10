using System.Threading;
using System.Threading.Tasks;

using FnbIdentity.Core.IdentityDto.DashboardIdentity;

namespace FnbIdentity.Core.IdentityServices.Interfaces
{
    public interface IDashboardIdentityService
    {
        public Task<DashboardIdentityDto> GetIdentityDashboardAsync(CancellationToken cancellationToken = default);
    }
}
