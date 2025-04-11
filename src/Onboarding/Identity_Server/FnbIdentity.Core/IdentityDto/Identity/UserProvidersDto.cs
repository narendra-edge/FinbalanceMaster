using FnbIdentity.Core.IdentityDto.Identity.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.IdentityDto.Identity
{
    public class UserProvidersDto<TUserProviderDto, TKey> : UserProviderDto<TKey>, IUserProvidersDto
        where TUserProviderDto : UserProviderDto<TKey>
    {
        public UserProvidersDto()
        {
            Providers = new List<TUserProviderDto>();
        }

        public List<TUserProviderDto> Providers { get; set; }

        List<IUserProviderDto> IUserProvidersDto.Providers => Providers.Cast<IUserProviderDto>().ToList();
    }
}
