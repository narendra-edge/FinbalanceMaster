using AutoMapper;
using FnbIdentity.Core.Dtos.Log;
using FnbIdentity.Infrastructure.Common;
using FnbIdentity.Infrastructure.Entities;

namespace FnbIdentity.Core.Mappers
{
    public class LogMapperProfile : Profile
    {
        public LogMapperProfile()
        {
            CreateMap<Log, LogDto>(MemberList.Destination)
                .ReverseMap();

            CreateMap<PagedList<Log>, LogsDto>(MemberList.Destination)
                .ForMember(x => x.Logs, opt => opt.MapFrom(src => src.Data));

            //CreateMap<AuditLog, AuditLogDto>(MemberList.Destination)
            //    .ReverseMap();

            //CreateMap<PagedList<AuditLog>, AuditLogsDto>(MemberList.Destination)
            //    .ForMember(x => x.Logs, opt => opt.MapFrom(src => src.Data));
        }
    }
}
