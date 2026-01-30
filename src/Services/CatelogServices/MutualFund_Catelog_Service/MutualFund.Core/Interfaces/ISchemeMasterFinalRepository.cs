using MutualFund.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Core.Interfaces
{
    public interface ISchemeMasterFinalRepository
    {
        Task<IEnumerable<SchemeMasterFinal>> GetAllAsync(CancellationToken ct = default);
        Task<SchemeMasterFinal?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(SchemeMasterFinal entity, CancellationToken ct = default);
        Task UpdateAsync(SchemeMasterFinal entity, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
