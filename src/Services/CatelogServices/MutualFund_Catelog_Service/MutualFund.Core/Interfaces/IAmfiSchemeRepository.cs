using MutualFund.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Core.Interfaces
{
    public interface IAmfiSchemeRepository
    {
        Task<IEnumerable<AmfiScheme>> GetAllAsync(CancellationToken ct = default);
        Task<AmfiScheme?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(AmfiScheme entity, CancellationToken ct = default);
        Task UpdateAsync(AmfiScheme entity, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        Task<AmfiScheme?> GetByCodeAsync(int code, CancellationToken ct = default);
    }
}
