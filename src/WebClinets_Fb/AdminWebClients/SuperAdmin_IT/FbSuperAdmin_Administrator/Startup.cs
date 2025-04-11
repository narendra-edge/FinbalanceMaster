using FbSuperAdmin_Administrator.Configuration.Database;
using FbSuperAdmin_Administrator.Helpers;
using FnbIdentity.Core.Shared.Dtos.Identity;
using FnbIdentity.Core.Shared.Dtos;
using FnbIdentity.Core.Shared.Helpers;
using FnbIdentity.Infrastructure.DbContexts;
using FnbIdentity.Infrastructure.Entities.Identity;
using FnbIdentity.UI.Helpers.DependencyInjection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using FnbIdentity.UI.Helpers.ApplicationBuilder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace FbSuperAdmin_Administrator
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            HostingEnvironment = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            
                // Adds the Duende IdentityServer Admin UI with custom options.
                services.AddIdentityServerAdminUI<IdentityServerConfigurationDbContext, IdentityServerPersistedGrantDbContext,
                AdminLogDbContext, AdminIdentityDbContext, IdentityServerDataProtectionDbContext,
                    UserIdentity, UserIdentityRole, UserIdentityUserClaim, UserIdentityUserRole,
                    UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken, string,
                    IdentityUserDto, IdentityRoleDto, IdentityUsersDto, IdentityRolesDto, IdentityUserRolesDto,
                    IdentityUserClaimsDto, IdentityUserProviderDto, IdentityUserProvidersDto, IdentityUserChangePasswordDto,
                    IdentityRoleClaimsDto, IdentityUserClaimDto, IdentityRoleClaimDto>(ConfigureUIOptions);

                // Monitor changes in Admin UI views
                services.AddAdminUIRazorRuntimeCompilation(HostingEnvironment);

                // Add email senders which is currently setup for SendGrid and SMTP
                services.AddEmailSenders(Configuration);
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseRouting();

            app.UseIdentityServerAdminUI();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapIdentityServerAdminUI();
                endpoint.MapIdentityServerAdminUIHealthChecks();
            });
        }

        public virtual void ConfigureUIOptions(IdentityServerAdminUIOptions options)
        {
            // Applies configuration from appsettings.
            options.BindConfiguration(Configuration);
            if (HostingEnvironment.IsDevelopment())
            {
                options.Security.UseDeveloperExceptionPage = true;
            }
           else
            {
                options.Security.UseHsts = true;
            }

            // Set migration assembly for application of db migrations
            var migrationsAssembly = MigrationAssemblyConfiguration.GetMigrationAssemblyByProvider(options.DatabaseProvider);
            options.DatabaseMigrations.SetMigrationsAssemblies(migrationsAssembly);

            // Use production DbContexts and auth services.
            options.Testing.IsStaging = false;
        }
    }
}
