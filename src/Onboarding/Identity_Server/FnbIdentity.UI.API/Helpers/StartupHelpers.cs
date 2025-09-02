using Duende.IdentityModel;

using FnbIdentity.Core.Extentions;
using FnbIdentity.Core.IdentityDto.Identity;
using FnbIdentity.Core.IdentityExtentions;
using FnbIdentity.Core.Shared.Helpers;
using FnbIdentity.Infrastructure.Configuration;
using FnbIdentity.Infrastructure.Extentions;
using FnbIdentity.Infrastructure.Helpers;
using FnbIdentity.Infrastructure.Interfaces;
using FnbIdentity.UI.API.Configuration;
using FnbIdentity.UI.API.Configuration.ApplicationParts;
using FnbIdentity.UI.API.Configuration.Constants;
using FnbIdentity.UI.API.ExceptionHandeling;
using FnbIdentity.UI.API.Helpers.Localization;
using FnbIdentity.UI.API.Mappers;
using FnbIdentity.UI.API.Resources;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Helpers
{
        public static class StartupHelpers
        {
            public static IServiceCollection AddAdminApiCors(this IServiceCollection services, AdminApiConfiguration adminApiConfiguration)
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(
                        builder =>
                        {
                            if (adminApiConfiguration.CorsAllowAnyOrigin)
                            {
                                builder.AllowAnyOrigin();
                            }
                            else
                            {
                                builder.WithOrigins(adminApiConfiguration.CorsAllowOrigins);
                            }

                            builder.AllowAnyHeader();
                            builder.AllowAnyMethod();
                        });
                });

                return services;
            }
            /// <summary>
            /// Register services for MVC
            /// </summary>
            /// <param name="services"></param>
            public static void AddMvcServices<TUserDto, TRoleDto,
                TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken,
                TUsersDto, TRolesDto, TUserRolesDto, TUserClaimsDto,
                TUserProviderDto, TUserProvidersDto, TUserChangePasswordDto, TRoleClaimsDto, TUserClaimDto, TRoleClaimDto>(
                this IServiceCollection services)
                where TUserDto : UserDto<TKey>, new()
                where TRoleDto : RoleDto<TKey>, new()
                where TUser : IdentityUser<TKey>
                where TRole : IdentityRole<TKey>
                where TKey : IEquatable<TKey>
                where TUserClaim : IdentityUserClaim<TKey>
                where TUserRole : IdentityUserRole<TKey>
                where TUserLogin : IdentityUserLogin<TKey>
                where TRoleClaim : IdentityRoleClaim<TKey>
                where TUserToken : IdentityUserToken<TKey>
                where TUsersDto : UsersDto<TUserDto, TKey>
                where TRolesDto : RolesDto<TRoleDto, TKey>
                where TUserRolesDto : UserRolesDto<TRoleDto, TKey>
                where TUserClaimsDto : UserClaimsDto<TUserClaimDto, TKey>
                where TUserProviderDto : UserProviderDto<TKey>
                where TUserProvidersDto : UserProvidersDto<TUserProviderDto, TKey>
                where TUserChangePasswordDto : UserChangePasswordDto<TKey>
                where TRoleClaimsDto : RoleClaimsDto<TRoleClaimDto, TKey>
                where TUserClaimDto : UserClaimDto<TKey>
                where TRoleClaimDto : RoleClaimDto<TKey>
            {
                services.AddLocalization(opts => { opts.ResourcesPath = ConfigurationConsts.ResourcesPath; });

                services.TryAddTransient(typeof(IGenericControllerLocalizer<>), typeof(GenericControllerLocalizer<>));

                services.AddControllersWithViews(o => { o.Conventions.Add(new GenericControllerRouteConvention()); })
                    .AddDataAnnotationsLocalization()
                    .ConfigureApplicationPartManager(m =>
                    {
                        m.FeatureProviders.Add(
                            new GenericTypeControllerFeatureProvider<TUserDto, TRoleDto,
                                TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken,
                                TUsersDto, TRolesDto, TUserRolesDto, TUserClaimsDto,
                                TUserProviderDto, TUserProvidersDto, TUserChangePasswordDto, TRoleClaimsDto, TUserClaimDto, TRoleClaimDto>());
                    });
            }
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
            public static void AddDbContexts<TConfigurationDbContext, TPersistedGrantDbContext,
                TLogDbContext, TIdentityDbContext, TDataProtectionDbContext>(this IServiceCollection services, IConfiguration configuration)
                where TIdentityDbContext : DbContext
                where TPersistedGrantDbContext : DbContext, IAdminPersistedGrantDbContext
                where TConfigurationDbContext : DbContext, IAdminConfigurationDbContext
                where TLogDbContext : DbContext, IAdminLogDbContext
                where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
            {
                var databaseProvider = configuration.GetSection(nameof(DatabaseProviderConfiguration)).Get<DatabaseProviderConfiguration>();
                var databaseMigrations = configuration.GetSection(nameof(DatabaseMigrationsConfiguration)).Get<DatabaseMigrationsConfiguration>() ?? new DatabaseMigrationsConfiguration();
                var connectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStringsConfiguration>();

                switch (databaseProvider.ProviderType)
                {
                    case DatabaseProviderType.SqlServer:
                        services.RegisterSqlServerDbContexts<TConfigurationDbContext, TPersistedGrantDbContext, TLogDbContext, TIdentityDbContext, TDataProtectionDbContext>(connectionStrings, databaseMigrations);
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
            /// <summary>
            /// Add authentication middleware for an API
            /// </summary>
            /// <typeparam name="TIdentityDbContext">DbContext for an access to Identity</typeparam>
            /// <typeparam name="TUser">Entity with User</typeparam>
            /// <typeparam name="TRole">Entity with Role</typeparam>
            /// <param name="services"></param>
            /// <param name="configuration"></param>
            public static void AddApiAuthentication<TIdentityDbContext, TUser, TRole>(this IServiceCollection services,
                IConfiguration configuration)
                where TIdentityDbContext : DbContext
                where TRole : class
                where TUser : class
            {
                var adminApiConfiguration = configuration.GetSection(nameof(AdminApiConfiguration)).Get<AdminApiConfiguration>();

                services.AddIdentityCore<TUser>(options => configuration.GetSection(nameof(IdentityOptions)).Bind(options))
                    .AddRoles<TRole>()
                    .AddEntityFrameworkStores<TIdentityDbContext>()
                    .AddDefaultTokenProviders();

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        options.Authority = adminApiConfiguration.IdentityServerBaseUrl;
                        options.RequireHttpsMetadata = adminApiConfiguration.RequireHttpsMetadata;
                        options.Audience = adminApiConfiguration.OidcApiName;
                    });
            }
            public static void AddAuthorizationPolicies(this IServiceCollection services)
            {
                var adminApiConfiguration = services.BuildServiceProvider().GetService<AdminApiConfiguration>();

                services.AddAuthorization(options =>
                {
                    options.AddPolicy(AuthorizationConsts.AdministrationPolicy,
                        policy =>
                            policy.RequireAssertion(context => context.User.HasClaim(c =>
                                    ((c.Type == JwtClaimTypes.Role && c.Value == adminApiConfiguration.AdministrationRole) ||
                                     (c.Type == $"client_{JwtClaimTypes.Role}" && c.Value == adminApiConfiguration.AdministrationRole))
                                ) && context.User.HasClaim(c => c.Type == JwtClaimTypes.Scope && c.Value == adminApiConfiguration.OidcApiName)
                            ));
                });
            }
            public static void AddIdSHealthChecks<TConfigurationDbContext, TPersistedGrantDbContext, TLogDbContext,
                TIdentityDbContext, TDataProtectionDbContext>(this IServiceCollection services, IConfiguration configuration,
                AdminApiConfiguration adminApiConfiguration)

               where TConfigurationDbContext : DbContext, IAdminConfigurationDbContext
               where TPersistedGrantDbContext : DbContext, IAdminPersistedGrantDbContext
               where TIdentityDbContext : DbContext
               where TLogDbContext : DbContext, IAdminLogDbContext
               where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
            {
                var configurationDbConnectionString = configuration.GetConnectionString(ConfigurationConsts.ConfigurationDbConnectionStringKey);
                var persistedGrantsDbConnectionString = configuration.GetConnectionString(ConfigurationConsts.PersistedGrantDbConnectionStringKey);
                var identityDbConnectionString = configuration.GetConnectionString(ConfigurationConsts.IdentityDbConnectionStringKey);
                var logDbConnectionString = configuration.GetConnectionString(ConfigurationConsts.AdminLogDbConnectionStringKey);
                var dataProtectionDbConnectionString = configuration.GetConnectionString(ConfigurationConsts.DataProtectionDbConnectionStringKey);

                var identityServerUri = adminApiConfiguration.IdentityServerBaseUrl;
                var healthChecksBuilder = services.AddHealthChecks()
                    .AddDbContextCheck<TConfigurationDbContext>("ConfigurationDbContext")
                    .AddDbContextCheck<TPersistedGrantDbContext>("PersistedGrantsDbContext")
                    .AddDbContextCheck<TIdentityDbContext>("IdentityDbContext")
                    .AddDbContextCheck<TLogDbContext>("LogDbContext")
                    .AddDbContextCheck<TDataProtectionDbContext>("DataProtectionDbContext")
                    .AddIdentityServer(new Uri(identityServerUri), "Identity Server");

            var serviceProvider = services.BuildServiceProvider();
                var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using (var scope = scopeFactory.CreateScope())
                {
                    var configurationTableName = DbContextHelpers.GetEntityTable<TConfigurationDbContext>(scope.ServiceProvider);
                    var persistedGrantTableName = DbContextHelpers.GetEntityTable<TPersistedGrantDbContext>(scope.ServiceProvider);
                    var identityTableName = DbContextHelpers.GetEntityTable<TIdentityDbContext>(scope.ServiceProvider);
                    var logTableName = DbContextHelpers.GetEntityTable<TLogDbContext>(scope.ServiceProvider);
                    var dataProtectionTableName = DbContextHelpers.GetEntityTable<TDataProtectionDbContext>(scope.ServiceProvider);

                    var databaseProvider = configuration.GetSection(nameof(DatabaseProviderConfiguration)).Get<DatabaseProviderConfiguration>();
                    switch (databaseProvider.ProviderType)
                    {
                        case DatabaseProviderType.SqlServer:
                            healthChecksBuilder
                                .AddSqlServer(configurationDbConnectionString, name: "ConfigurationDb",
                                    healthQuery: $"SELECT TOP 1 * FROM dbo.[{configurationTableName}]")
                                .AddSqlServer(persistedGrantsDbConnectionString, name: "PersistentGrantsDb",
                                    healthQuery: $"SELECT TOP 1 * FROM dbo.[{persistedGrantTableName}]")
                                .AddSqlServer(identityDbConnectionString, name: "IdentityDb",
                                    healthQuery: $"SELECT TOP 1 * FROM dbo.[{identityTableName}]")
                                .AddSqlServer(logDbConnectionString, name: "LogDb",
                                    healthQuery: $"SELECT TOP 1 * FROM dbo.[{logTableName}]")
                                .AddSqlServer(dataProtectionDbConnectionString, name: "DataProtectionDb",
                                healthQuery: $"SELECT TOP 1 * FROM dbo.[{dataProtectionTableName}]");
                            break;
                        //case DatabaseProviderType.PostgreSQL:
                        //    healthChecksBuilder
                        //        .AddNpgSql(configurationDbConnectionString, name: "ConfigurationDb",
                        //            healthQuery: $"SELECT * FROM \"{configurationTableName}\" LIMIT 1")
                        //        .AddNpgSql(persistedGrantsDbConnectionString, name: "PersistentGrantsDb",
                        //            healthQuery: $"SELECT * FROM \"{persistedGrantTableName}\" LIMIT 1")
                        //        .AddNpgSql(identityDbConnectionString, name: "IdentityDb",
                        //            healthQuery: $"SELECT * FROM \"{identityTableName}\" LIMIT 1")
                        //        .AddNpgSql(logDbConnectionString, name: "LogDb",
                        //            healthQuery: $"SELECT * FROM \"{logTableName}\" LIMIT 1")
                        //        .AddNpgSql(dataProtectionDbConnectionString, name: "DataProtectionDb",
                        //            healthQuery: $"SELECT * FROM \"{dataProtectionTableName}\"  LIMIT 1");
                        //    break;
                        //case DatabaseProviderType.MySql:
                        //    healthChecksBuilder
                        //        .AddMySql(configurationDbConnectionString, name: "ConfigurationDb")
                        //        .AddMySql(persistedGrantsDbConnectionString, name: "PersistentGrantsDb")
                        //        .AddMySql(identityDbConnectionString, name: "IdentityDb")
                        //        .AddMySql(logDbConnectionString, name: "LogDb")
                        //        .AddMySql(dataProtectionDbConnectionString, name: "DataProtectionDb");
                        //    break;
                        default:
                            throw new NotImplementedException($"Health checks not defined for database provider {databaseProvider.ProviderType}");
                    }
                }
            }

        public static void AddForwardHeaders(this IApplicationBuilder app)
        {
            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.All
            };

            forwardingOptions.KnownNetworks.Clear();
            forwardingOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardingOptions);
        }

             public static void AddIdentityServerAdminApi<TIdentityDbContext, TIdentityServerConfigurationDbContext, TPersistedGrantDbContext, TIdentityServerDataProtectionDbContext, TAdminLogDbContext, TUserDto, TRoleDto, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken,
            TUsersDto, TRolesDto, TUserRolesDto, TUserClaimsDto,
            TUserProviderDto, TUserProvidersDto, TUserChangePasswordDto, TRoleClaimsDto, TUserClaimDto, TRoleClaimDto>(this IServiceCollection services, IConfiguration configuration, AdminApiConfiguration adminApiConfiguration)
            where TPersistedGrantDbContext : DbContext, IAdminPersistedGrantDbContext
            where TIdentityDbContext : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
            where TUserDto : UserDto<TKey>, new()
            where TRoleDto : RoleDto<TKey>, new()
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
            where TKey : IEquatable<TKey>
            where TUserClaim : IdentityUserClaim<TKey>
            where TUserRole : IdentityUserRole<TKey>
            where TUserLogin : IdentityUserLogin<TKey>
            where TRoleClaim : IdentityRoleClaim<TKey>
            where TUserToken : IdentityUserToken<TKey>
            where TUsersDto : UsersDto<TUserDto, TKey>
            where TRolesDto : RolesDto<TRoleDto, TKey>
            where TUserRolesDto : UserRolesDto<TRoleDto, TKey>
            where TUserClaimsDto : UserClaimsDto<TUserClaimDto, TKey>
            where TUserProviderDto : UserProviderDto<TKey>
            where TUserProvidersDto : UserProvidersDto<TUserProviderDto, TKey>
            where TUserChangePasswordDto : UserChangePasswordDto<TKey>
            where TRoleClaimsDto : RoleClaimsDto<TRoleClaimDto, TKey>
            where TUserClaimDto : UserClaimDto<TKey>
            where TRoleClaimDto : RoleClaimDto<TKey>
            where TIdentityServerDataProtectionDbContext : DbContext, IDataProtectionKeyContext
            where TIdentityServerConfigurationDbContext : DbContext, IAdminConfigurationDbContext
            where TAdminLogDbContext : DbContext, IAdminLogDbContext
            //where TAdminAuditLogDbContext : IAuditLoggingDbContext<AuditLog>, IAuditLoggingDbContext<TAuditLog>
            //where TAuditLog : AuditLog, new()
        {
            services.AddDataProtection<TIdentityServerDataProtectionDbContext>(configuration);

            services.AddScoped<ControllerExceptionFilterAttribute>();
            services.AddScoped<IApiErrorResources, ApiErrorResources>();

            var profileTypes = new HashSet<Type>
            {
                typeof(IdentityMapperProfile<TRoleDto, TUserRolesDto, TKey, TUserClaimsDto, TUserClaimDto, TUserProviderDto, TUserProvidersDto, TUserChangePasswordDto, TRoleClaimDto, TRoleClaimsDto>)
            };

            services.AddConfigureAdminAspNetIdentitySchema(configuration);

            services.AddAdminAspNetIdentityServices<TIdentityDbContext, TPersistedGrantDbContext,
                TUserDto, TRoleDto, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken,
                TUsersDto, TRolesDto, TUserRolesDto, TUserClaimsDto,
                TUserProviderDto, TUserProvidersDto, TUserChangePasswordDto, TRoleClaimsDto, TUserClaimDto, TRoleClaimDto>(profileTypes);

            services.AddAdminServices<TIdentityServerConfigurationDbContext, TPersistedGrantDbContext, TAdminLogDbContext>();

            services.AddAdminApiCors(adminApiConfiguration);

            services.AddMvcServices<TUserDto, TRoleDto, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken,
                TUsersDto, TRolesDto, TUserRolesDto, TUserClaimsDto,
                TUserProviderDto, TUserProvidersDto, TUserChangePasswordDto, TRoleClaimsDto, TUserClaimDto, TRoleClaimDto>();

            //services.AddAuditEventLogging<TAdminAuditLogDbContext, TAuditLog>(configuration);
             
        }

        }
}

