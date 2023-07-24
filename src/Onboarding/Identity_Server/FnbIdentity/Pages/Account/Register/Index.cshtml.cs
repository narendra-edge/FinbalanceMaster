using FnbIdentity.Model;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FnbIdentity.Pages.Account.Register
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
             
        }
        [BindProperty]
        public RegisterViewModel Input { get; set; }

        public async Task<IActionResult> OnGet(string returnUrl)
        {
            List<string> roles = new()
            {
                "Admin",
                "investor"
            };
            ViewData["roles_message"] = roles;
            Input = new RegisterViewModel
            {
                ReturnUrl = returnUrl,
            };
            return Page();
        }
        public async Task<IActionResult> OnPost(string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    EmailConfirmed = true,
                    
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(Input.RoleName).GetAwaiter().GetResult())
                    {
                        var userRole = new IdentityRole
                        {
                            Name = Input.RoleName,
                            NormalizedName = Input.RoleName,
                        };
                        await _roleManager.CreateAsync(userRole);
                    }
                    await _userManager.AddToRoleAsync(user, Input.RoleName);

                    await _userManager.AddClaimsAsync(user, new Claim[]
                    {
                        new Claim(JwtClaimTypes.Name, Input.Name),
                        new Claim(JwtClaimTypes.Email, Input.Email),
                        new Claim(JwtClaimTypes.Role, Input.RoleName)
                    });

                    var loginresult = await _signInManager.PasswordSignInAsync(Input.Email,
                                      Input.Password, false, lockoutOnFailure: true);
                    if(loginresult.Succeeded) 
                    { 
                      if(Url.IsLocalUrl(Input.ReturnUrl))
                        {
                            return Redirect(Input.ReturnUrl);
                        }
                      else if (string.IsNullOrEmpty(Input.ReturnUrl)) 
                        {
                            return Redirect("~/");
                        }
                      else
                        {
                            throw new Exception("invalid return Url");
                        }
                    }
                }

            }
            return Page();
        }
    }
}
