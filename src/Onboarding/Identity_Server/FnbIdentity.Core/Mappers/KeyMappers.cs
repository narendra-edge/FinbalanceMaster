using AutoMapper;
using Duende.IdentityServer.EntityFramework.Entities;
using FnbIdentity.Core.Dtos.Keys;
using FnbIdentity.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Mappers
{
    public static class KeyMappers
    {
        internal static IMapper Mapper { get; }

        static KeyMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<KeyMapperProfile>())
                .CreateMapper();
        }

        public static KeyDto ToModel(this Key key)
        {
            return key == null ? null : Mapper.Map<KeyDto>(key);
        }

        public static KeysDto ToModel(this PagedList<Key> grant)
        {
            return grant == null ? null : Mapper.Map<KeysDto>(grant);
        }
    }
}
