using FnbIdentity.Core.IdentityDto.DashboardIdentity;
using System.Threading;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityServices.Interfaces
{
    public interface IDashboardIdentityService
    {
        public Task<DashboardIdentityDto> GetIdentityDashboardAsync(CancellationToken cancellationToken = default);
    }
}
