
using FnbIdentity.Core.Shared.Configuration.Common;
using FnbIdentity.Core.Shared.Configuration.Email;
using FnbIdentity.Core.Shared.Email;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Shared.Helpers
{
    public static class StartupHelpers
    {
        /// <summary>
        /// Add email senders - configuration of sendgrid, smtp senders
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddEmailSenders(this IServiceCollection services, IConfiguration configuration)
        {
            var smtpConfiguration = configuration.GetSection(nameof(SmtpConfiguration)).Get<SmtpConfiguration>();
            var sendGridConfiguration = configuration.GetSection(nameof(SendGridConfiguration)).Get<SendGridConfiguration>();

            if (sendGridConfiguration != null && !string.IsNullOrWhiteSpace(sendGridConfiguration.ApiKey))
            {
                services.AddSingleton<ISendGridClient>(_ => new SendGridClient(sendGridConfiguration.ApiKey));
                services.AddSingleton(sendGridConfiguration);
                services.AddTransient<IEmailSender, SendGridEmailSender>();
            }
            else if (smtpConfiguration != null && !string.IsNullOrWhiteSpace(smtpConfiguration.Host))
            {
                services.AddSingleton(smtpConfiguration);
                services.AddTransient<IEmailSender, SmtpEmailSender>();
            }
            else
            {
                services.AddSingleton<IEmailSender, LogEmailSender>();
            }
        }
        public static void AddDataProtection<TDbContext>(this IServiceCollection services, IConfiguration configuration)
                    where TDbContext : DbContext, IDataProtectionKeyContext
        {
            AddDataProtection<TDbContext>(
                services,
                configuration.GetSection(nameof(DataProtectionConfiguration)).Get<DataProtectionConfiguration>());
               // configuration.GetSection(nameof(AzureKeyVaultConfiguration)).Get<AzureKeyVaultConfiguration>());
        }

        public static void AddDataProtection<TDbContext>(this IServiceCollection services,
                                         DataProtectionConfiguration dataProtectionConfiguration) 
            
            where TDbContext : DbContext, IDataProtectionKeyContext
        {
           var dataProtectionBuilder = services.AddDataProtection()
                .SetApplicationName("FnbIdentiy.IdentityServer")
                .PersistKeysToDbContext<TDbContext>();

            //if (dataProtectionConfiguration.ProtectKeysWithAzureKeyVault)
            //{
                //if (azureKeyVaultConfiguration.UseClientCredentials)
                //{
                //    dataProtectionBuilder.ProtectKeysWithAzureKeyVault(
                //        new Uri(azureKeyVaultConfiguration.DataProtectionKeyIdentifier),
                //        new ClientSecretCredential(azureKeyVaultConfiguration.TenantId,
                //            azureKeyVaultConfiguration.ClientId, azureKeyVaultConfiguration.ClientSecret));
                //}
                //else
                //{
                //    dataProtectionBuilder.ProtectKeysWithAzureKeyVault(new Uri(azureKeyVaultConfiguration.DataProtectionKeyIdentifier), new DefaultAzureCredential());
                //}
          //  }
        }
    }
}
