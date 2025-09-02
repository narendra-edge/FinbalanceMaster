using FnbIdentity.Infrastructure.Entities;
using FnbIdentity.Infrastructure.Interfaces;
using FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.RepositoryIdentityServer
{
    public class DashboardRepository<TDbContext> : IDashboardRepository
    where TDbContext : DbContext, IAdminConfigurationDbContext
    {
        protected readonly TDbContext DbContext;

        public DashboardRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<DashboardDataView> GetDashboardIdentityServerAsync(int auditLogsLastNumberOfDays, CancellationToken cancellationToken = default)
        {
            return new DashboardDataView
            {
                ClientsTotal = await DbContext.Clients.CountAsync(cancellationToken: cancellationToken),
                ApiResourcesTotal = await DbContext.ApiResources.CountAsync(cancellationToken: cancellationToken),
                ApiScopesTotal = await DbContext.ApiScopes.CountAsync(cancellationToken: cancellationToken),
                IdentityResourcesTotal = await DbContext.IdentityResources.CountAsync(cancellationToken: cancellationToken),
                IdentityProvidersTotal = await DbContext.IdentityProviders.CountAsync(cancellationToken: cancellationToken)
            };
        }
    }
}
