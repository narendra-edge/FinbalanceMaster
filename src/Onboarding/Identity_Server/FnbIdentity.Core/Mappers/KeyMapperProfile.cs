using AutoMapper;
using Duende.IdentityServer.EntityFramework.Entities;
using FnbIdentity.Core.Dtos.Keys;
using FnbIdentity.Infrastructure.Common;
using FnbIdentity.Core.Dtos.Grants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FnbIdentity.Core.Mappers
{
    public class KeyMapperProfile : Profile
    {
        public KeyMapperProfile()
        {
            CreateMap<PagedList<Key>, KeysDto>(MemberList.Destination)
                .ForMember(x => x.Keys,
                    opt => opt.MapFrom(src => src.Data));

            CreateMap<Key, KeyDto>(MemberList.Destination)
                .ReverseMap();
        }
    }
}
