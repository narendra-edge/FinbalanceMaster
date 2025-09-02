using FnbIdentity.Core.IdentityDto.DashboardIdentity;
using FnbIdentity.Core.IdentityServices.Interfaces;
using FnbIdentity.Infrastructure.RepositoryIdentity.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityServices
{
    public class DashboardIdentityService : IDashboardIdentityService
    {
        protected readonly IDashboardIdentityRepository DashboardIdentityRepository;

        public DashboardIdentityService(IDashboardIdentityRepository dashboardIdentityRepository)
        {
            DashboardIdentityRepository = dashboardIdentityRepository;
        }

        public async Task<DashboardIdentityDto> GetIdentityDashboardAsync(CancellationToken cancellationToken = default)
        {
            return new DashboardIdentityDto
            {
                RolesTotal = await DashboardIdentityRepository.GetRolesTotalCountAsync(cancellationToken),
                UsersTotal = await DashboardIdentityRepository.GetUsersTotalCountAsync(cancellationToken)
            };
        }
    }
}
