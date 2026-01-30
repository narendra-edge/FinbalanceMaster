using Microsoft.EntityFrameworkCore;
using MutualFund.Core.Entities;
using MutualFund.Core.Interfaces;
using MutualFund.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Infrastructure.Repositories
{
    public class AmfiSchemeRepository : IAmfiSchemeRepository
    {
        private readonly MutualFundDbContext _context;

        public AmfiSchemeRepository(MutualFundDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AmfiScheme>> GetAllAsync(CancellationToken ct = default) =>
        await _context.AmfiScheme.AsNoTracking().ToListAsync(ct);

        public async Task<AmfiScheme?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await _context.AmfiScheme.FindAsync(new object[] { id }, ct);

        public async Task AddAsync(AmfiScheme entity, CancellationToken ct = default)
        {
            _context.AmfiScheme.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(AmfiScheme entity, CancellationToken ct = default)
        {
            _context.AmfiScheme.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var e = await _context.AmfiScheme.FindAsync(new object[] { id }, ct);
            if (e != null)
            {
                _context.AmfiScheme.Remove(e);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<AmfiScheme?> GetByCodeAsync(int code, CancellationToken ct = default) =>
            await _context.AmfiScheme.AsNoTracking().FirstOrDefaultAsync(a => a.Code == code, ct);
    }
}
