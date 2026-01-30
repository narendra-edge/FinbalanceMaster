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
    public class RtaSchemeDataRepository : IRtaSchemeDataRepository
    {
        private readonly MutualFundDbContext _context;
        public RtaSchemeDataRepository(MutualFundDbContext context)
        {
            _context = context;
        }
        

        public async Task<IEnumerable<RtaSchemeData>> GetAllAsync(CancellationToken ct = default) =>
            await _context.RtaSchemeData.AsNoTracking().ToListAsync(ct);

        public async Task<RtaSchemeData?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await _context.RtaSchemeData.FindAsync(new object[] { id }, ct);

        public async Task AddAsync(RtaSchemeData entity, CancellationToken ct = default)
        {
            _context.RtaSchemeData.Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(RtaSchemeData entity, CancellationToken ct = default)
        {
            _context.RtaSchemeData.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var e = await _context.RtaSchemeData.FindAsync(new object[] { id }, ct);
            if (e != null)
            {
                _context.RtaSchemeData.Remove(e);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<RtaSchemeData?> GetByRtaSchemeCodeAsync(string rtaSchemeCode, CancellationToken ct = default) =>
            await _context.RtaSchemeData.AsNoTracking().FirstOrDefaultAsync(x => x.RtaSchemeCode == rtaSchemeCode, ct);

        public async Task<IEnumerable<RtaSchemeData>> SearchByNormalizedNameAsync(string normalized, int limit = 50, CancellationToken ct = default) =>
            await _context.RtaSchemeData.AsNoTracking()
                .Where(x => x.NormalizeSchemeName != null && EF.Functions.ILike(x.NormalizeSchemeName, $"%{normalized}%"))
                .Take(limit)
                .ToListAsync(ct);
    }
}
