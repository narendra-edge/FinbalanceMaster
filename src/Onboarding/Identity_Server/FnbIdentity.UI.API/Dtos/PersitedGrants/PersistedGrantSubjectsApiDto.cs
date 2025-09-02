using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Dtos.PersitedGrants
{
    public class PersistedGrantSubjectsApiDto
    {
        public PersistedGrantSubjectsApiDto()
        {
            PersistedGrants = new List<PersistedGrantSubjectApiDto>();
        }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public List<PersistedGrantSubjectApiDto> PersistedGrants { get; set; }
    }
}
