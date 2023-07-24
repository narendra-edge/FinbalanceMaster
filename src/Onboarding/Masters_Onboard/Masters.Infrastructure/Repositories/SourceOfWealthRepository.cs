using AutoMapper;
using Masters.Core.Interfaces.Repositories;
using Masters.Infrastructure.Context;
using Masters.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Repositories
{
    public class SourceOfWealthRepository : ISourceOfWealthRepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public SourceOfWealthRepository(CatelogDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));    
        }
        public async Task<IEnumerable<SourceOfWealth>> GetAllSourceOfWealth()
        {
             var sourceOfWealth = await _dbContext.SourceOfWealths.ToListAsync().ConfigureAwait(false);
            if(sourceOfWealth != null)
            {
                return _mapper.Map<IEnumerable<SourceOfWealth>>(sourceOfWealth);
            }
            return null;
        }

        public async Task<SourceOfWealth> GetSourceOfWealthById(int SrcId)
        {
            var sourceOfWealth = await _dbContext.SourceOfWealths.FindAsync(SrcId);
            if (sourceOfWealth != null)
            {
                return _mapper.Map<SourceOfWealth>(sourceOfWealth);
            }
            return null;
        }
    }
}
