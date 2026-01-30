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
    public class SchemeMappingRepository : ISchemeMappingRepository
    {
        private readonly MutualFundDbContext _context;

        public SchemeMappingRepository(MutualFundDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SchemeMapping>> GetAllAsync(CancellationToken ct = default) =>
         await _context.SchemeMapping.AsNoTracking().ToListAsync(ct);

        public async Task<SchemeMapping?> GetByIdAsync(long id, CancellationToken ct = default) =>
            await _context.SchemeMapping.FindAsync(new object[] { id }, ct);

        public async Task AddAsync(SchemeMapping entity, CancellationToken ct = default)
        {
            _context.SchemeMapping.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(SchemeMapping entity, CancellationToken ct = default)
        {
            _context.SchemeMapping.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(long id, CancellationToken ct = default)
        {
            var e = await _context.SchemeMapping.FindAsync(new object[] { id }, ct);
            if (e != null) { _context.SchemeMapping.Remove(e); await _context.SaveChangesAsync(ct); }
        }
    }
}
