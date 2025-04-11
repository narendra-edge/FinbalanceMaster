using FnbIdentity.Core.Dtos.Events.Log;
using FnbIdentity.Core.Dtos.Logs;
using FnbIdentity.Core.Mappers;
using FnbIdentity.Core.Services.Interfaces;
using FnbIdentity.Infrastructure.RepositoryIdentityServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Services
{
    public class LogService : ILogService
    {
        protected readonly ILogRepository Repository;
       // protected readonly IAuditEventLogger AuditEventLogger;

        public LogService(ILogRepository repository)
        {
            Repository = repository;
          //  AuditEventLogger = auditEventLogger;
        }

        public virtual async Task<LogsDto> GetLogsAsync(string search, int page = 1, int pageSize = 10)
        {
            var pagedList = await Repository.GetLogsAsync(search, page, pageSize);
            var logs = pagedList.ToModel();

          //  await AuditEventLogger.LogEventAsync(new LogsRequestedEvent());

            return logs;
        }

        public virtual async Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan)
        {
            await Repository.DeleteLogsOlderThanAsync(deleteOlderThan);

           // await AuditEventLogger.LogEventAsync(new LogsDeletedEvent(deleteOlderThan));
        }
    }
}
