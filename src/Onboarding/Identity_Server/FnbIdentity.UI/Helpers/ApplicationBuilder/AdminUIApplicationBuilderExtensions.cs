using FnbIdentity.UI.Configuration;
using FnbIdentity.UI.Configuration.Constants;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.Helpers.ApplicationBuilder
{
	public static class AdminUIApplicationBuilderExtensions
	{
		/// <summary>
		/// Adds the FnbIdentity Duende IdentityServer Admin UI to the pipeline of this application. This method must be called 
		/// between UseRouting() and UseEndpoints().
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IApplicationBuilder UseIdentityServerAdminUI(this IApplicationBuilder app)
		{
			app.UseRoutingDependentMiddleware(app.ApplicationServices.GetRequiredService<TestingConfiguration>());

			return app;
		}

        /// <summary>
        /// Maps the FnbIdentity Duende IdentityServer Admin UI to the routes of this application.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="patternPrefix"></param>
        public static IEndpointConventionBuilder MapIdentityServerAdminUI(this IEndpointRouteBuilder endpoint, string patternPrefix = "/")
		{
			return endpoint.MapAreaControllerRoute(CommonConsts.AdminUIArea, CommonConsts.AdminUIArea, 
				patternPrefix + "{controller=Home}/{action=Index}/{id?}");
		}

		/// <summary>
		/// Maps the FnbIdentity IdentityServer Admin UI health checks to the routes of this application.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="pattern"></param>
		public static IEndpointConventionBuilder MapIdentityServerAdminUIHealthChecks(this IEndpointRouteBuilder endpoint, string pattern = "/health", Action<HealthCheckOptions> configureAction = null)
		{
			var options = new HealthCheckOptions
			{
				ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
			};

			configureAction?.Invoke(options);

			return endpoint.MapHealthChecks(pattern, options);
		}
	}
}
