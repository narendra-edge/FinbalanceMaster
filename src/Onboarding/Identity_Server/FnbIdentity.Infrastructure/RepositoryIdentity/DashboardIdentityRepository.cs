using FnbIdentity.Infrastructure.RepositoryIdentity.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace FnbIdentity.Infrastructure.RepositoryIdentity
{
    public class DashboardIdentityRepository<TUser, TKey, TRole> : IDashboardIdentityRepository
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
    {
        protected readonly UserManager<TUser> UserManager;
        protected readonly RoleManager<TRole> RoleManager;
        public DashboardIdentityRepository(UserManager<TUser> userManager, RoleManager<TRole> roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }
        public Task<int> GetRolesTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return UserManager.Users.CountAsync(cancellationToken: cancellationToken);
        }

        public Task<int> GetUsersTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return RoleManager.Roles.CountAsync(cancellationToken);
        }
    }
}
