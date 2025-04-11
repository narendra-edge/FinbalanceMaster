using FnbIdentity.Core.Shared.Configuration.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace FnbIdentity.STS.Identity.Helpers
{
    public class UserResolver<TUser> where TUser : class
    {
        private readonly UserManager<TUser> _userManager;
        private readonly LoginResolutionPolicy _policy;

        public UserResolver(UserManager<TUser> userManager, LoginConfiguration configuration)
        {
            _userManager = userManager;
            _policy = configuration.ResolutionPolicy;
        }

        public async Task<TUser> GetUserAsync(string login)
        {
            switch (_policy)
            {
                case LoginResolutionPolicy.Username:
#pragma warning disable CS8603 // Possible null reference return.
                    return await _userManager.FindByNameAsync(login);
#pragma warning restore CS8603 // Possible null reference return.
                case LoginResolutionPolicy.Email:
#pragma warning disable CS8603 // Possible null reference return.
                    return await _userManager.FindByEmailAsync(login);
#pragma warning restore CS8603 // Possible null reference return.
                default:
#pragma warning disable CS8603 // Possible null reference return.
                    return null;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }
    }
}
