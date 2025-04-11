using AutoMapper;
using Duende.IdentityServer.EntityFramework.Entities;
using FnbIdentity.Core.Dtos.Configuration;
using FnbIdentity.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Mappers
{
    public class ApiScopeMapperProfile : Profile
    {
        public ApiScopeMapperProfile()
        {
            // entity to model
            CreateMap<ApiScope, ApiScopeDto>(MemberList.Destination)
                .ForMember(x => x.UserClaims, opt => opt.MapFrom(src => src.UserClaims.Select(x => x.Type)));

            CreateMap<ApiScopeProperty, ApiScopePropertyDto>(MemberList.Destination)
                .ReverseMap();

            CreateMap<ApiScopeProperty, ApiScopePropertiesDto>(MemberList.Destination)
                .ForMember(dest => dest.Key, opt => opt.Condition(srs => srs != null))
                .ForMember(x => x.ApiScopePropertyId, opt => opt.MapFrom(x => x.Id))
                .ForMember(x => x.ApiScopeId, opt => opt.MapFrom(x => x.Scope.Id));

            // PagedLists
            CreateMap<PagedList<ApiScope>, ApiScopesDto>(MemberList.Destination)
                .ForMember(x => x.Scopes, opt => opt.MapFrom(src => src.Data));

            CreateMap<PagedList<ApiScopeProperty>, ApiScopePropertiesDto>(MemberList.Destination)
                .ForMember(x => x.ApiScopeProperties, opt => opt.MapFrom(src => src.Data));

            // model to entity
            CreateMap<ApiScopeDto, ApiScope>(MemberList.Source)
                .ForMember(x => x.UserClaims, opts => opts.MapFrom(src => src.UserClaims.Select(x => new ApiScopeClaim { Type = x })));

            CreateMap<ApiScopePropertiesDto, ApiScopeProperty>(MemberList.Source)
                .ForMember(x => x.Scope, dto => dto.MapFrom(src => new ApiScope { Id = src.ApiScopeId }))
                .ForMember(x => x.ScopeId, dto => dto.MapFrom(src => src.ApiScopeId))
                .ForMember(x => x.Id, opt => opt.MapFrom(src => src.ApiScopePropertyId));
        }
    }
}
