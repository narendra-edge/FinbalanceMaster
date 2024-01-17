using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using FnbIdentity.Database;
using FnbIdentity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace FnbIdentity
{
    public class SeedData
    {
        public static void EnsureSeedData(WebApplication app)
        {
            using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();

                var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                context.Database.Migrate();
                EnsureSeedData(context);
                EnsureUsers(scope);

            }
        }
        private static  void EnsureSeedData(ConfigurationDbContext context)
        {
            
            if (! context.Clients.Any())
            {
                Log.Debug("Clients being populated");
                foreach (var client in Config.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }

                 context.SaveChanges();
            }
            else
            {
                Log.Debug("Clients already populated");
            }

            if (!context.IdentityResources.Any())
            {
                Log.Debug("IdentityResources being populated");
                foreach (var resource in Config.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }

                 context.SaveChanges();
            }
            else
            {
                Log.Debug("IdentityResources already populated");
            }

            if (!context.ApiScopes.Any())
            {
                Log.Debug("ApiScopes being populated");
                foreach (var resource in Config.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }

                 context.SaveChanges();
            }
            else
            {
                Log.Debug("ApiScopes already populated");
            }

            if (!context.ApiResources.Any())
            {
                Log.Debug("ApiResources being populated");
                foreach (var resource in Config.ApiResources.ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                } 
                 context.SaveChanges();
            }
            else
            {
                Log.Debug("ApiResources already populated");
            }

            if (!context.IdentityProviders.Any())
            {
                Log.Debug("OIDC IdentityProviders being populated");
                context.IdentityProviders.Add(new OidcProvider
                {
                    Scheme = "demoidsrv",
                    DisplayName = "IdentityServer",
                    Authority = "https://demo.duendesoftware.com",
                    ClientId = "login",
                }.ToEntity());
                context.SaveChanges();
            }
            else
            {
                Log.Debug("OIDC IdentityProviders already populated");
            }
        }


        private static void EnsureUsers(IServiceScope scope)
        {
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var investor = roleMgr.FindByNameAsync("investor").Result;
            if (investor == null)
            {
                investor = new IdentityRole
                {
                    Name = "investor",
                    
                };
                _ = roleMgr.CreateAsync(investor).Result;
            }
            var admin = roleMgr.FindByNameAsync("admin").Result;
            if (admin == null)
            {
                investor = new IdentityRole
                {
                    Name = "admin",
                    
                };
                _ = roleMgr.CreateAsync(admin).Result;
            }
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var superadmin = userMgr.FindByNameAsync("superadmin").Result;
            
            if (superadmin == null)
            {
                superadmin = new ApplicationUser
                {
                    UserName = "Superadmin",
                    Email = "edgefintrack@email.com",
                    EmailConfirmed = true,
                   
                };
                var result = userMgr.CreateAsync(superadmin, "Pass123$").Result;

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                if (!userMgr.IsInRoleAsync(superadmin, admin.Name).Result)
                {
                    _ = userMgr.AddToRoleAsync(superadmin, admin.Name).Result;
                }

                Log.Debug("superadmin created");
            }
            else
            {
                Log.Debug("superadmin already exists");
            }

            var naren = userMgr.FindByNameAsync("naren").Result;
            if (naren == null)
            {
                naren = new ApplicationUser
                {
                    UserName = "narendra",
                    Email = "naren2050@email.com",
                    EmailConfirmed = true,
                    
                };
                var result = userMgr.CreateAsync(naren, "Pass123$").Result;

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                if (!userMgr.IsInRoleAsync(naren, investor.Name).Result)
                {
                    _ = userMgr.AddToRoleAsync(naren, investor.Name).Result;
                }

                Log.Debug("naren created");
            }
            else
            {
                Log.Debug("naren already exists");
            }
        }
    }
}

