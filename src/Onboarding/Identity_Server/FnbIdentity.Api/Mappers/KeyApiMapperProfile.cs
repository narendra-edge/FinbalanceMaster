using AutoMapper;
using FnbIdentity.Api.Dtos.Key;
using FnbIdentity.Core.Dtos.Keys;

namespace FnbIdentity.Api.Mappers
{
    public class KeyApiMapperProfile : Profile
    {
        public KeyApiMapperProfile()
        {
            CreateMap<KeyDto, KeyApiDto>(MemberList.Destination)
                .ReverseMap();

            CreateMap<KeysDto, KeysApiDto>(MemberList.Destination)
                .ReverseMap();
        }
    }
}
