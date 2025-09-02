using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Mappers
{
    public static class KeyApiMappers
    {
        static KeyApiMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<KeyApiMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        public static T ToKeyApiModel<T>(this object source)
        {
            return Mapper.Map<T>(source);
        }
    }
}
