using FnbIdentity.Core.Dtos.IdentityProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Services.Interfaces
{
    public interface IIdentityProviderService
    {
        Task<IdentityProvidersDto> GetIdentityProvidersAsync(string search, int page = 1, int pageSize = 10);

        Task<IdentityProviderDto> GetIdentityProviderAsync(int identityProviderId);

        Task<bool> CanInsertIdentityProviderAsync(IdentityProviderDto identityProvider);

        Task<int> AddIdentityProviderAsync(IdentityProviderDto identityProvider);

        Task<int> UpdateIdentityProviderAsync(IdentityProviderDto identityProvider);

        Task<int> DeleteIdentityProviderAsync(IdentityProviderDto identityProvider);
    }
}
