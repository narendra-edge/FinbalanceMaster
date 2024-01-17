using FnbIdentity.Model;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace FnbIdentity.Factories
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public ApplicationUserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager,
            IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var claimsIdentity = await base.GenerateClaimsAsync(user);
            if(user.BirthDate != null)
            {
                claimsIdentity.AddClaim(new Claim(JwtClaimTypes.BirthDate, user.BirthDate));
            }
            
            if (user.ZoneInfo != null)
            {
                claimsIdentity.AddClaim(new Claim(JwtClaimTypes.ZoneInfo, user.ZoneInfo));
            }

            return claimsIdentity;

        }
    }
}
