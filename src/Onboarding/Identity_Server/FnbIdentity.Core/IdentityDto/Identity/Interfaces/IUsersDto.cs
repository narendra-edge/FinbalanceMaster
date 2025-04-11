using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityDto.Identity.Interfaces
{
    public interface IUsersDto
    {
        int PageSize { get; set; }
        int TotalCount { get; set; }
        List<IUserDto> Users { get; }
    }
}
