using Masters.Api;
using Masters.Api.Middleware;
using Masters.Infrastructure.Context;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Serilog;
using System.Net;
using System.Text.Json.Serialization;

//Log.Logger = new LoggerConfiguration()
//    .WriteTo.Console()
//    .CreateBootstrapLogger();



try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers().AddNewtonsoftJson(Options =>
    {
        Options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        Options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });
    builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.ConfigureSwagger();

    var Configuration = builder.Configuration;
    var connectionString = Configuration.GetConnectionString("FbCatelogData");

    builder.Host.UseSerilog((ctx, lc) => lc
           //.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
           //.Enrich.FromLogContext()
           .ReadFrom.Configuration(ctx.Configuration));
    builder.Services.AddDbContext<CatelogDbContext>(options =>
    {
        options.UseSqlServer(connectionString);
    });
    builder.Services.ConfigureServices();
    builder.Services.ConfigureHealthCheck(builder.Configuration);

    var app = builder.Build();

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    if (app.Environment.IsDevelopment())
    {
        //app.UseHttpCodeAndLogMiddleware();
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHttpCodeAndLogMiddleware();
        app.UseHsts();
    }
    app.UseSwagger(options =>
            options.RouteTemplate = $"swagger/{ApiConstants.ServiceName}/{{documentName}}/swagger.json");
    app.UseSwaggerUI(
            options =>
            {
                options.RoutePrefix = $"swagger/{ApiConstants.ServiceName}";

                foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }

            });
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSerilogRequestLogging();
    app.UseAuthorization();
  
    app.UseHealthChecks("/api/health", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    app.UseHealthChecksUI(options =>
    {
        options.UIPath = "/healthcheck-ui";

    });
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException") // https://github.com/dotnet/runtime/issues/60600
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}


