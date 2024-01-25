using Duende.IdentityServer.EntityFramework.Entities;
using FnbIdentity.Infrastructure.Common;


namespace FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces
{
    public interface IIdentityProviderRepository
    {
        Task<PagedList<IdentityProvider>> GetIdentityProvidersAsync(string search, int page = 1, int pageSize = 10);

        Task<IdentityProvider> GetIdentityProviderAsync(int identityProviderId);

        Task<bool> CanInsertIdentityProviderAsync(IdentityProvider identityProvider);

        Task<int> AddIdentityProviderAsync(IdentityProvider identityProvider);

        Task<int> UpdateIdentityProviderAsync(IdentityProvider identityProvider);

        Task<int> DeleteIdentityProviderAsync(IdentityProvider identityProvider);

        Task<int> SaveAllChangesAsync();

        bool AutoSaveChanges { get; set; }
    }
}
