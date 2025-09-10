using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FnbIdentity.Core.Dtos.Events.IdentityProvider;
using FnbIdentity.Core.Dtos.IdentityProviders;
using FnbIdentity.Core.ExceptionHandling;
using FnbIdentity.Core.Mappers;
using FnbIdentity.Core.Resources;
using FnbIdentity.Core.Services.Interfaces;
using FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces;


namespace FnbIdentity.Core.Services
{
    public class IdentityProviderService : IIdentityProviderService
    {
        protected readonly IIdentityProviderRepository IdentityProviderRepository;
        protected readonly IIdentityProviderServiceResources IdentityProviderServiceResources;
        //protected readonly IAuditEventLogger AuditEventLogger;

        public IdentityProviderService(IIdentityProviderRepository identityProviderRepository,
            IIdentityProviderServiceResources identityProviderServiceResources)
          //  IAuditEventLogger auditEventLogger)
        {
            IdentityProviderRepository = identityProviderRepository;
            IdentityProviderServiceResources = identityProviderServiceResources;
           // AuditEventLogger = auditEventLogger;
        }

        public virtual async Task<IdentityProvidersDto> GetIdentityProvidersAsync(string search, int page = 1, int pageSize = 10)
        {
            var pagedList = await IdentityProviderRepository.GetIdentityProvidersAsync(search, page, pageSize);
            var identityProviderDto = pagedList.ToModel();

            //await AuditEventLogger.LogEventAsync(new IdentityProvidersRequestedEvent(identityProviderDto));

            return identityProviderDto;
        }

        public virtual async Task<IdentityProviderDto> GetIdentityProviderAsync(int identityProviderId)
        {
            var identityProvider = await IdentityProviderRepository.GetIdentityProviderAsync(identityProviderId);
            if (identityProvider == null) throw new UserFriendlyErrorPageException(string.Format(IdentityProviderServiceResources.IdentityProviderDoesNotExist().Description, identityProviderId));

            var identityProviderDto = identityProvider.ToModel();

            //await AuditEventLogger.LogEventAsync(new IdentityProviderRequestedEvent(identityProviderDto));

            return identityProviderDto;
        }

        public virtual async Task<bool> CanInsertIdentityProviderAsync(IdentityProviderDto identityProvider)
        {
            var entity = identityProvider.ToEntity();

            return await IdentityProviderRepository.CanInsertIdentityProviderAsync(entity);
        }

        public virtual async Task<int> AddIdentityProviderAsync(IdentityProviderDto identityProvider)
        {
            var canInsert = await CanInsertIdentityProviderAsync(identityProvider);
            if (!canInsert)
            {
                throw new UserFriendlyViewException(string.Format(IdentityProviderServiceResources.IdentityProviderExistsValue().Description, identityProvider.Scheme), IdentityProviderServiceResources.IdentityProviderExistsKey().Description, identityProvider);
            }

            var entity = identityProvider.ToEntity();

            var saved = await IdentityProviderRepository.AddIdentityProviderAsync(entity);

            //await AuditEventLogger.LogEventAsync(new IdentityProviderAddedEvent(identityProvider));

            return saved;
        }

        public virtual async Task<int> UpdateIdentityProviderAsync(IdentityProviderDto identityProvider)
        {
            var canInsert = await CanInsertIdentityProviderAsync(identityProvider);
            if (!canInsert)
            {
                throw new UserFriendlyViewException(string.Format(IdentityProviderServiceResources.IdentityProviderExistsValue().Description, identityProvider.Scheme), IdentityProviderServiceResources.IdentityProviderExistsKey().Description, identityProvider);
            }

            var originalIdentityProvider = await GetIdentityProviderAsync(identityProvider.Id);

            if (identityProvider.Properties == null)
            {
                identityProvider.Properties = new Dictionary<int, IdentityProviderPropertyDto>();
            }

            var entity = identityProvider.ToEntity();

            var updated = await IdentityProviderRepository.UpdateIdentityProviderAsync(entity);

            //await AuditEventLogger.LogEventAsync(new IdentityProviderUpdatedEvent(originalIdentityProvider, identityProvider));

            return updated;
        }

        public virtual async Task<int> DeleteIdentityProviderAsync(IdentityProviderDto identityProvider)
        {
            var entity = identityProvider.ToEntity();

            var deleted = await IdentityProviderRepository.DeleteIdentityProviderAsync(entity);

           // await AuditEventLogger.LogEventAsync(new IdentityProviderDeletedEvent(identityProvider));

            return deleted;
        }
    }
}
