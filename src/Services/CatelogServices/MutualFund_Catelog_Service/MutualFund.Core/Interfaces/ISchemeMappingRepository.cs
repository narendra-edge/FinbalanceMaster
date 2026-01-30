using MutualFund.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Core.Interfaces
{
    public interface ISchemeMappingRepository
    {
        Task<IEnumerable<SchemeMapping>> GetAllAsync(CancellationToken ct = default);
        Task<SchemeMapping?> GetByIdAsync(long id, CancellationToken ct = default);
        Task AddAsync(SchemeMapping entity, CancellationToken ct = default);
        Task UpdateAsync(SchemeMapping entity, CancellationToken ct = default);
        Task DeleteAsync(long id, CancellationToken ct = default);
    }
}
