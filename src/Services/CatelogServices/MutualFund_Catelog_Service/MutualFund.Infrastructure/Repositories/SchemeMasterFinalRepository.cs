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
    public class SchemeMasterFinalRepository : ISchemeMasterFinalRepository
    {
        private readonly MutualFundDbContext _context;

        public SchemeMasterFinalRepository(MutualFundDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SchemeMasterFinal>> GetAllAsync(CancellationToken ct = default) =>
         await _context.SchemeMasterFinal.AsNoTracking().ToListAsync(ct);

        public async Task<SchemeMasterFinal?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await _context.SchemeMasterFinal.FindAsync(new object[] { id }, ct);

        public async Task AddAsync(SchemeMasterFinal entity, CancellationToken ct = default)
        {
            _context.SchemeMasterFinal.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(SchemeMasterFinal entity, CancellationToken ct = default)
        {
            _context.SchemeMasterFinal.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var e = await _context.SchemeMasterFinal.FindAsync(new object[] { id }, ct);
            if (e != null) { _context.SchemeMasterFinal.Remove(e); await _context.SaveChangesAsync(ct); }
        }
    }
}
