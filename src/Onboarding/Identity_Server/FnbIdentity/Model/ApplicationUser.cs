using Microsoft.AspNetCore.Identity;

namespace FnbIdentity.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string? BirthDate { get; set; }
        public string? ZoneInfo { get; set; }
    }
}
