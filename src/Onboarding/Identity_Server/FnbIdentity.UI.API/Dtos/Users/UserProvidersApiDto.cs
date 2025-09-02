using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Dtos.Users
{
    public class UserProvidersApiDto<TKey>
    {
        public UserProvidersApiDto()
        {
            Providers = new List<UserProviderApiDto<TKey>>();
        }

        public List<UserProviderApiDto<TKey>> Providers { get; set; }
    }
}
