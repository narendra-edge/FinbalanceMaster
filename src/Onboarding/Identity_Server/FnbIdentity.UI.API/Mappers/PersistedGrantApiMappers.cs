using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Mappers
{
    public static class PersistedGrantApiMappers
    {
        static PersistedGrantApiMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<PersistedGrantApiMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        public static T ToPersistedGrantApiModel<T>(this object source)
        {
            return Mapper.Map<T>(source);
        }
    }
}
