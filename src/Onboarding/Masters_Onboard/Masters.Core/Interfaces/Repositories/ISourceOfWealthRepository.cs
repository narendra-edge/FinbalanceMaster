using Masters.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Interfaces.Repositories
{
    public interface ISourceOfWealthRepository
    {
        Task<IEnumerable<SourceOfWealth>> GetAllSourceOfWealth();
        Task<SourceOfWealth> GetSourceOfWealthById(int SrcId);
    }
}
