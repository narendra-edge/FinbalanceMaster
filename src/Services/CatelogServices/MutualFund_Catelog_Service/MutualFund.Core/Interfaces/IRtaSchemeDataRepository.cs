using MutualFund.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Core.Interfaces
{
    public interface IRtaSchemeDataRepository
    {
        Task<IEnumerable<RtaSchemeData>> GetAllAsync(CancellationToken ct = default);
        Task<RtaSchemeData?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(RtaSchemeData entity, CancellationToken ct = default);
        Task UpdateAsync(RtaSchemeData entity, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        // Custom queries
        Task<RtaSchemeData?> GetByRtaSchemeCodeAsync(string rtaSchemeCode, CancellationToken ct = default);
        Task<IEnumerable<RtaSchemeData>> SearchByNormalizedNameAsync(string normalized, int limit = 50, CancellationToken ct = default);
    }
}
