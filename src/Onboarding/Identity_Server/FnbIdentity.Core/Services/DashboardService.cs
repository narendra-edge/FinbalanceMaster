using FnbIdentity.Core.Dtos.Dashboard;
using FnbIdentity.Core.Services.Interfaces;
using FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Services
{
    public class DashboardService : IDashboardService
    {
        protected readonly IDashboardRepository DashboardRepository;
        // protected readonly IAuditLogService AuditLogService;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            DashboardRepository = dashboardRepository;
            // AuditLogService = auditLogService;
        }

        public async Task<DashboardDto> GetDashboardIdentityServerAsync( CancellationToken cancellationToken = default)
        {
           var dashBoardData = await DashboardRepository.GetDashboardIdentityServerAsync(cancellationToken);
            //  var auditLogs = await AuditLogService.GetDashboardAuditLogsAsync(auditLogsLastNumberOfDays, cancellationToken);
            //  var auditLogsAverage = await AuditLogService.GetDashboardAuditLogsAverageAsync(auditLogsLastNumberOfDays, cancellationToken);

            return new DashboardDto
            {
                ClientsTotal = dashBoardData.ClientsTotal,
                ApiResourcesTotal = dashBoardData.ApiResourcesTotal,
                ApiScopesTotal = dashBoardData.ApiScopesTotal,
                IdentityResourcesTotal = dashBoardData.IdentityResourcesTotal,
                //AuditLogsAvg = auditLogsAverage,
                //  AuditLogsPerDaysTotal = auditLogs,
                IdentityProvidersTotal = dashBoardData.IdentityProvidersTotal
            };
        }
    }
}
