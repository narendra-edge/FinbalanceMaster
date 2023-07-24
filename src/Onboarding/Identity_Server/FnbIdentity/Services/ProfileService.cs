using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using FnbIdentity.Model;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace FnbIdentity.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaims;

        public ProfileService(UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              IUserClaimsPrincipalFactory<ApplicationUser> userClaims)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userClaims = userClaims;

        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            string sub = context.Subject.GetSubjectId();
            ApplicationUser user = await _userManager.FindByIdAsync(sub);

            ClaimsPrincipal userClaims = await _userClaims.CreateAsync(user);
            List<Claim> claims = userClaims.Claims.ToList();
            claims = claims.Where(u => context.RequestedClaimTypes.Contains(u.Type)).ToList();
            if (_userManager.SupportsUserRole)
            {
                IList<string> roles = await _userManager.GetRolesAsync(user);
                foreach (var rolename in roles)
                {
                    claims.Add(new Claim(JwtClaimTypes.Role, rolename));
                }

            }

            context.IssuedClaims = claims;
        }
   

        public async  Task IsActiveAsync(IsActiveContext context)
        {
            string sub = context.Subject.GetSubjectId();
            ApplicationUser user = await  _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
                
        }
    }
}