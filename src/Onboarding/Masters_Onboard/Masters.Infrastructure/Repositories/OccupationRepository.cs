using AutoMapper;
using Masters.Core.Interfaces.Repositories;
using Masters.Core.Models;
using Masters.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Repositories
{
    public class OccupationRepository : IOccupationRepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public OccupationRepository(CatelogDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<IEnumerable<Occupation>> GetAllOccupation()
        {
            var occupation = await _dbContext.Occupations.ToListAsync().ConfigureAwait(false);
            if (occupation != null)
            {
                return _mapper.Map<IEnumerable<Occupation>>(occupation);
            }
            return null;
        }

        public async Task<Occupation> GetOccupationById(int id)
        {
            var occupation = await _dbContext.Occupations.FindAsync(id);
            if (occupation != null)
            {
                return _mapper.Map<Occupation>(occupation);
            }
            return null;
        }
    }
}
