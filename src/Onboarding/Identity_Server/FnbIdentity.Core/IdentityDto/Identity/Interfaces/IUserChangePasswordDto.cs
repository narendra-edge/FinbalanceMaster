using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityDto.Identity.Interfaces
{
    public interface IUserChangePasswordDto : IBaseUserChangePasswordDto
    {
        string UserName { get; set; }
        string Password { get; set; }
        string ConfirmPassword { get; set; }
    }
}
