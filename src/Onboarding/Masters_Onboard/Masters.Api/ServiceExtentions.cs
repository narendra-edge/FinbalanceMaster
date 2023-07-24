
using Masters.Api.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Writers;

namespace Masters.Api
{
    public static class ServiceExtentions
    {
        public static void ConfigureCors(this IServiceCollection services) 
        {
            services.AddCors(option =>
            {
                option.AddPolicy("corsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

            });           
        }     
        
        public static void  ConfigureHealthCheck(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddHealthChecks()
                .AddSqlServer(configuration["ConnectionStrings:FbCatelogData"], healthQuery: "select 1", name: "SQL Server", failureStatus: HealthStatus.Unhealthy, tags: new[] { "FbCatelogData", "Database" })
                .AddCheck<RemoteHealthCheck>("Remote endpoint Health Check", failureStatus: HealthStatus.Unhealthy)
                .AddCheck<MemoryHealthCheck>($"Instrument Service MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"Exchange Service  MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"PinCodeMaster Service  MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"DistrictMaster MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"StateMaster Service  MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"CountryCode Service  MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"TaxStatus Service  MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"Occupation Service  MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"StatusOfWealth Service  MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"IncomeDetails Service  MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })
                .AddCheck<MemoryHealthCheck>($"UBOCode Service  MemoryCheck", failureStatus: HealthStatus.Unhealthy, tags: new[] { "masters" })

                .AddUrlGroup(new Uri("https://localhost:44360/api/masters/v1/heartbeat/ping"), name: "base URL", failureStatus: HealthStatus.Unhealthy);

            services.AddHealthChecksUI(opt =>
             {
                 opt.SetEvaluationTimeInSeconds(10);
                 opt.MaximumHistoryEntriesPerEndpoint(60);
                 opt.SetApiMaxActiveRequests(1);
                 opt.AddHealthCheckEndpoint("masters", "/api/health");
             })
                .AddInMemoryStorage();
        }
    }
}
