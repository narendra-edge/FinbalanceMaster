using Duende.IdentityServer.EntityFramework.Options;
using FnbIdentity;
using FnbIdentity.Database;
using FnbIdentity.Factories;
using FnbIdentity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

Log.Logger = new LoggerConfiguration()
  .WriteTo.Console()
  .CreateBootstrapLogger();

Log.Information("Starting up");


var builder = WebApplication.CreateBuilder(args);

//var Configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>((ServiceProvider,dbContextOptionsBuilder) =>
{
    dbContextOptionsBuilder.UseSqlServer(ServiceProvider.
        GetRequiredService<IConfiguration>().GetConnectionString("FbIdentityUser"), SqlServerOptionsAction);
});

builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
                .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    options.EmitStaticAudienceClaim = true;

}).AddServerSideSessions()
  .AddConfigurationStore(configurationsStoreOptions =>
  {
      configurationsStoreOptions.ResolveDbContextOptions = ResolveDbContextOptions;
  })
  .AddOperationalStore(OperationalStoreOptions =>
  {
      OperationalStoreOptions.ResolveDbContextOptions = ResolveDbContextOptions;
  });
 //.AddAspNetIdentity<ApplicationUser>();
 
                 //.AddProfileService<ProfileService>();
//builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddAuthentication();

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.MinimumLevel.Debug()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
      .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
      .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
      .MinimumLevel.Override("System", LogEventLevel.Warning)
      .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
        theme: AnsiConsoleTheme.Code)
      .Enrich.FromLogContext();
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseIdentityServer();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

if (args.Contains("/seed"))
{
    Log.Information("seeding database...");
     SeedData.EnsureSeedData(app);
    Log.Information("done seeding database. exiting.");
    return;
}

app.Run();
void SqlServerOptionsAction(SqlServerDbContextOptionsBuilder sqlServerDbContextOptionsBuilder )
   {
    sqlServerDbContextOptionsBuilder.MigrationsAssembly(typeof(Config).GetTypeInfo().Assembly.GetName().Name);
   }
void ResolveDbContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder dbContextOptionsBuilder ) 
    {
    dbContextOptionsBuilder.UseSqlServer(serviceProvider.GetRequiredService<IConfiguration>()
    .GetConnectionString("FbIdentityServer"), SqlServerOptionsAction);
    } 