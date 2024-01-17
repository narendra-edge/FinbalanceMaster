

using FnbIdentity.Infrastructure.Configuration;
using FnbIdentity.Infrastructure.Extentions;
using FnbIdentity.Infrastructure.Interfaces;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FnbIdentity.Api
{
    public static class ServiceExtentions
    {
        /// <summary>
        /// Register DbContexts for IdentityServer ConfigurationStore and PersistedGrants, Identity and Logging
        /// Configure the connection strings in AppSettings.json
        /// </summary>
        /// <typeparam name="TConfigurationDbContext"></typeparam>
        /// <typeparam name="TPersistedGrantDbContext"></typeparam>
        /// <typeparam name="TLogDbContext"></typeparam>
        /// <typeparam name="TIdentityDbContext"></typeparam>
        /// <typeparam name="TDataProtectionDbContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDbContexts< TConfigurationDbContext, TPersistedGrantDbContext,
            TLogDbContext, TIdentityDbContext, TDataProtectionDbContext>(this IServiceCollection services, IConfiguration configuration)
            
            where TPersistedGrantDbContext : DbContext, IAdminPersistedGrantDbContext
            where TConfigurationDbContext : DbContext, IAdminConfigurationDbContext
            where TLogDbContext : DbContext, IAdminLogDbContext
            where TIdentityDbContext : DbContext
            where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
        {
            var databaseProvider = configuration.GetSection(nameof(DatabaseProviderConfiguration)).Get<DatabaseProviderConfiguration>();
            var databaseMigrations = configuration.GetSection(nameof(DatabaseMigrationsConfiguration)).Get<DatabaseMigrationsConfiguration>() ?? new DatabaseMigrationsConfiguration();
            var connectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStringsConfiguration>();

            switch (databaseProvider?.ProviderType)
            {
                case DatabaseProviderType.SqlServer:
                    services.RegisterSqlServerDbContexts< TConfigurationDbContext, TPersistedGrantDbContext, TLogDbContext, TIdentityDbContext, TDataProtectionDbContext>(connectionStrings!, databaseMigrations);
                    break;
                //case DatabaseProviderType.PostgreSQL:
                //    services.RegisterNpgSqlDbContexts<TIdentityDbContext, TConfigurationDbContext, TPersistedGrantDbContext, TLogDbContext, TAuditLoggingDbContext, TDataProtectionDbContext, TAuditLog>(connectionStrings, databaseMigrations);
                //    break;
                //case DatabaseProviderType.MySql:
                //    services.RegisterMySqlDbContexts<TIdentityDbContext, TConfigurationDbContext, TPersistedGrantDbContext, TLogDbContext, TAuditLoggingDbContext, TDataProtectionDbContext, TAuditLog>(connectionStrings, databaseMigrations);
                //    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseProvider.ProviderType), $@"The value needs to be one of {string.Join(", ", Enum.GetNames(typeof(DatabaseProviderType)))}.");
            }
        }
    }
}
