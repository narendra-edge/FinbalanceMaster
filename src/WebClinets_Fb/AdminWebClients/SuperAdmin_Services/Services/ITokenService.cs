using IdentityModel.Client;

namespace FbSuperadmin.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GetToken(string scope);
    }
}
