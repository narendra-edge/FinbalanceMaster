using FnbIdentity.UI.Helpers.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FbSuperAdmin_Administrator.Configuration.Test
{
    public class StartupTest :Startup
    {
        public StartupTest(IWebHostEnvironment env, IConfiguration configuration) : base(env, configuration)
        {
        }

        public override void ConfigureUIOptions(IdentityServerAdminUIOptions options)
        {
            base.ConfigureUIOptions(options);

            // Use staging DbContexts and auth services.
            options.Testing.IsStaging = true;
        }
    }
}
