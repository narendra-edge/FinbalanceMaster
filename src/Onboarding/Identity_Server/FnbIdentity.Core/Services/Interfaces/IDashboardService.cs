using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FnbIdentity.Core.Dtos.Dashboard;


namespace FnbIdentity.Core.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardIdentityServerAsync(
        CancellationToken cancellationToken = default);
    }
}
