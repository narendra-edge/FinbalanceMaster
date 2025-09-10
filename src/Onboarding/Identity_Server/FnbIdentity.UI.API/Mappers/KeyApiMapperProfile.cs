using AutoMapper;
using FnbIdentity.Core.Dtos.Keys;
using FnbIdentity.UI.API.Dtos.Key;


namespace FnbIdentity.UI.API.Mappers
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
