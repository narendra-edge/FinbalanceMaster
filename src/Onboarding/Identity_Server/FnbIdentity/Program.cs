using Duende.IdentityServer.Services;
using FnbIdentity;
using FnbIdentity.Database;
using FnbIdentity.Model;
using FnbIdentity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
  .WriteTo.Console()
  .CreateBootstrapLogger();

Log.Information("Starting up");


var builder = WebApplication.CreateBuilder(args);

var Configuration = builder.Configuration;

var connectionString = Configuration.GetConnectionString("FbIdentityServer");

var migrationsAssembly = typeof(Config).Assembly.GetName().Name;


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqloptions => sqloptions.MigrationsAssembly(migrationsAssembly));
});

builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    options.EmitStaticAudienceClaim = true;

}).AddServerSideSessions()
  .AddConfigurationStore(options => options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
  opt => opt.MigrationsAssembly(migrationsAssembly)))
  .AddOperationalStore(options => options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
    opt => opt.MigrationsAssembly(migrationsAssembly)))
 .AddAspNetIdentity<ApplicationUser>();
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

app.UseIdentityServer();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

//if (args.Contains("/seed"))
//{
//    Log.Information("Seeding database...");
  //  SeedData.EnsureSeedData(app);
 //   Log.Information("Done seeding database. Exiting.");
  //  return;
//}


app.Run();
