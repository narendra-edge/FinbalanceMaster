using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Mappers
{
    public static class IdentityResourceApiMappers
    {
        static IdentityResourceApiMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceApiMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        public static T ToIdentityResourceApiModel<T>(this object source)
        {
            return Mapper.Map<T>(source);
        }
    }
}
