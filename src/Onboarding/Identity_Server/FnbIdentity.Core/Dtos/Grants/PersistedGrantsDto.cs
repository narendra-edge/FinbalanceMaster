using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Grants
{
    public class PersistedGrantsDto
    {
        public PersistedGrantsDto()
        {
            PersistedGrants = new List<PersistedGrantDto>();
        }

        public string? SubjectId { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public List<PersistedGrantDto> PersistedGrants { get; set; }
    }
}
