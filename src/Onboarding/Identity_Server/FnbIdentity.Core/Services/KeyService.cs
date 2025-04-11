using FnbIdentity.Core.Dtos.Events.Key;
using FnbIdentity.Core.Dtos.Keys;
using FnbIdentity.Core.ExceptionHandling;
using FnbIdentity.Core.Mappers;
using FnbIdentity.Core.Resources;
using FnbIdentity.Core.Services.Interfaces;
using FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Services
{
    public class KeyService : IKeyService
    {
        protected readonly IKeyRepository KeyRepository;
        //protected readonly IAuditEventLogger AuditEventLogger;
        protected readonly IKeyServiceResources KeyServiceResources;

        public KeyService(IKeyRepository keyRepository, IKeyServiceResources keyServiceResources)
        {
            KeyRepository = keyRepository;
           // AuditEventLogger = auditEventLogger;
            KeyServiceResources = keyServiceResources;
        }

        public async Task<KeysDto> GetKeysAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var keys = await KeyRepository.GetKeysAsync(page, pageSize, cancellationToken);

            var keysDto = keys.ToModel();

           // await AuditEventLogger.LogEventAsync(new KeysRequestedEvent(keysDto));

            return keysDto;
        }

        public async Task<KeyDto> GetKeyAsync(string id, CancellationToken cancellationToken = default)
        {
            var key = await KeyRepository.GetKeyAsync(id, cancellationToken);

            if (key == default)
            {
                throw new UserFriendlyErrorPageException(string.Format(KeyServiceResources.KeyDoesNotExist().Description, id));
            }

            var keyDto = key.ToModel();

          //  await AuditEventLogger.LogEventAsync(new KeyRequestedEvent(keyDto));

            return keyDto;
        }

        public Task<bool> ExistsKeyAsync(string id, CancellationToken cancellationToken = default)
        {
            return KeyRepository.ExistsKeyAsync(id, cancellationToken);
        }

        public async Task DeleteKeyAsync(string id, CancellationToken cancellationToken = default)
        {
            var key = await GetKeyAsync(id, cancellationToken);

          //  await AuditEventLogger.LogEventAsync(new KeyDeletedEvent(key));

            await KeyRepository.DeleteKeyAsync(id, cancellationToken);
        }
    }
}
