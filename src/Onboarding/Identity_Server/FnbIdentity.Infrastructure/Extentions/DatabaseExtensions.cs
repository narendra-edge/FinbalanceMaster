using Duende.IdentityServer.EntityFramework.Storage;
using FnbIdentity.Infrastructure.Configuration;
using FnbIdentity.Infrastructure.Interfaces;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace FnbIdentity.Infrastructure.Extentions
{
    public static class DatabaseExtensions
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
        /// <param name="connectionStrings"></param>
        /// <param name="databaseMigrations"></param>

        public static void RegisterSqlServerDbContexts<TConfigurationDbContext, TPersistedGrantDbContext, 
                                                     TLogDbContext, TIdentityDbContext, TDataProtectionDbContext>
                                                     (this IServiceCollection services, ConnectionStringsConfiguration connectionStrings,
                                                      DatabaseMigrationsConfiguration databaseMigrations)
            
            where TConfigurationDbContext : DbContext, IAdminConfigurationDbContext
            where TPersistedGrantDbContext : DbContext, IAdminPersistedGrantDbContext
            where TLogDbContext : DbContext, IAdminLogDbContext
            where TIdentityDbContext : DbContext          
            where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
            
        {
            var migrationsAssembly =  typeof(DatabaseExtensions).GetTypeInfo().Assembly.GetName().Name;

            // Config DB for identity
            services.AddDbContext<TIdentityDbContext>(options =>
            options.UseSqlServer(connectionStrings.IdentityDbConnection,
            sql => sql.MigrationsAssembly(databaseMigrations.IdentityDbMigrationsAssembly ?? migrationsAssembly)));

            // Configuration DB from Existing connection
            services.AddConfigurationDbContext<TConfigurationDbContext>(options => options.ConfigureDbContext = b => b.UseSqlServer
            (connectionStrings.ConfigurationDbConnection, sql => sql.MigrationsAssembly(databaseMigrations.ConfigurationDbMigrationsAssembly ?? migrationsAssembly)));

            // Operation DB from Exiting Connection
            services.AddOperationalDbContext<TPersistedGrantDbContext>(options => options.ConfigureDbContext = b => b.UseSqlServer
            (connectionStrings.PersistedGrantDbConnection, sql => sql.MigrationsAssembly(databaseMigrations.PersistedGrantDbMigrationsAssembly ?? migrationsAssembly)));
        
           // Log  DB from Existing Connection
           services.AddDbContext<TLogDbContext>(options => options.UseSqlServer(connectionStrings.AdminLogDbConnection,
                optionsSql => optionsSql.MigrationsAssembly(databaseMigrations.AdminLogDbMigrationsAssembly ?? migrationsAssembly)));
            
            // Config DB for identity
            services.AddDbContext<TIdentityDbContext>(options =>
            options.UseSqlServer(connectionStrings.IdentityDbConnection,
            sql => sql.MigrationsAssembly(databaseMigrations.IdentityDbMigrationsAssembly ?? migrationsAssembly)));

            // DataProtectionKey DB from existing connection
            if (!string.IsNullOrEmpty(connectionStrings.DataProtectionDbConnection))
                services.AddDbContext<TDataProtectionDbContext>(options => options.UseSqlServer(connectionStrings.DataProtectionDbConnection,
                    optionsSql => optionsSql.MigrationsAssembly(databaseMigrations.DataProtectionDbMigrationsAssembly ?? migrationsAssembly)));

        }
    }
}
