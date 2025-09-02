using AutoMapper;
using FnbIdentity.Core.Dtos.Log;
using FnbIdentity.Infrastructure.Common;
using FnbIdentity.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
