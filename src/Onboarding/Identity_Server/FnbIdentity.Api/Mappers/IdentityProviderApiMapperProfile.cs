using AutoMapper;
using FnbIdentity.Api.Dtos.IdentityProvider;
using FnbIdentity.Core.Dtos.IdentityProviders;
using System.Collections.Generic;
using System.Linq;

namespace FnbIdentity.Api.Mappers
{
    public class IdentityProviderApiMapperProfile : Profile
    {
        public IdentityProviderApiMapperProfile()
        {
            // map to api dto
            CreateMap<IdentityProviderDto, IdentityProviderApiDto>(MemberList.Destination)
                .ForMember(apiDto => apiDto.IdentityProviderProperties,
                    opts => opts.ConvertUsing(PropertiesConverter.Converter, x => x.Properties))
                ;

            // map from api dto
            CreateMap<IdentityProviderApiDto, IdentityProviderDto>(MemberList.Destination)
                .ForMember(dto => dto.Properties,
                    opts => opts.ConvertUsing(PropertiesConverter.Converter, x => x.IdentityProviderProperties));

            CreateMap<IdentityProvidersDto, IdentityProvidersApiDto>(MemberList.Destination)
                .ReverseMap();
        }

        class PropertiesConverter :
            IValueConverter<Dictionary<int, IdentityProviderPropertyDto>, Dictionary<string, string>>,
            IValueConverter<Dictionary<string, string>, Dictionary<int, IdentityProviderPropertyDto>>
        {
            public static PropertiesConverter Converter = new PropertiesConverter();

            public Dictionary<string, string> Convert(Dictionary<int, IdentityProviderPropertyDto> sourceMember, ResolutionContext context)
            {
                var dict = sourceMember.ToDictionary(x => x.Value.Name, dto => dto.Value.Value);
                return dict;
            }

            public Dictionary<int, IdentityProviderPropertyDto> Convert(Dictionary<string, string> sourceMember, ResolutionContext context)
            {
                var index = 0;
                var dict = sourceMember.Select(i => new IdentityProviderPropertyDto { Name = i.Key, Value = i.Value });
                return dict.ToDictionary(_ => index++, item => item);
            }
        }
    }
}
