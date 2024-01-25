using ApiResource = Duende.IdentityServer.EntityFramework.Entities.ApiResource;
using FnbIdentity.Infrastructure.Common;
using Duende.IdentityServer.EntityFramework.Entities;

namespace FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces
{
    public interface IApiResourceRepository
    {
        Task<PagedList<ApiResource>> GetApiResourcesAsync(string search, int page = 1, int pageSize = 10);

        Task<ApiResource> GetApiResourceAsync(int apiResourceId);

        Task<PagedList<ApiResourceProperty>> GetApiResourcePropertiesAsync(int apiResourceId, int page = 1, int pageSize = 10);

        Task<ApiResourceProperty> GetApiResourcePropertyAsync(int apiResourcePropertyId);

        Task<int> AddApiResourcePropertyAsync(int apiResourceId, ApiResourceProperty apiResourceProperty);

        Task<int> DeleteApiResourcePropertyAsync(ApiResourceProperty apiResourceProperty);

        Task<bool> CanInsertApiResourcePropertyAsync(ApiResourceProperty apiResourceProperty);

        Task<int> AddApiResourceAsync(ApiResource apiResource);

        Task<int> UpdateApiResourceAsync(ApiResource apiResource);

        Task<int> DeleteApiResourceAsync(ApiResource apiResource);

        Task<bool> CanInsertApiResourceAsync(ApiResource apiResource);

        Task<PagedList<ApiResourceSecret>> GetApiSecretsAsync(int apiResourceId, int page = 1, int pageSize = 10);

        Task<int> AddApiSecretAsync(int apiResourceId, ApiResourceSecret apiSecret);

        Task<ApiResourceSecret> GetApiSecretAsync(int apiSecretId);

        Task<int> DeleteApiSecretAsync(ApiResourceSecret apiSecret);

        Task<int> SaveAllChangesAsync();
    }
}
