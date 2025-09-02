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
    public static class LogMappers
    {
        internal static IMapper Mapper { get; }

        static LogMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<LogMapperProfile>())
                .CreateMapper();
        }

        public static LogDto ToModel(this Log log)
        {
            return Mapper.Map<LogDto>(log);
        }

        public static LogsDto ToModel(this PagedList<Log> logs)
        {
            return Mapper.Map<LogsDto>(logs);
        }

        //public static AuditLogsDto ToModel<TAuditLog>(this PagedList<TAuditLog> auditLogs)
        //    where TAuditLog : AuditLog
        //{
        //    return Mapper.Map<AuditLogsDto>(auditLogs);
        //}

        //public static AuditLogDto ToModel(this AuditLog auditLog)
        //{
        //    return Mapper.Map<AuditLogDto>(auditLog);
        //}

        public static Log ToEntity(this LogDto log)
        {
            return Mapper.Map<Log>(log);
        }
    }
}
