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
    public class StateMasterRepository : IStateMasterRepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public StateMasterRepository(CatelogDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<IEnumerable<StateMaster>> GetAllStateMaster()
        {
            var statemaster = await _dbContext.StateMasters.Include(x => x.CountryCode).ToListAsync().ConfigureAwait(false);
            if (statemaster != null)
            {
                return _mapper.Map<IEnumerable<StateMaster>>(statemaster);
            }
            return null;
        }

        public async Task<StateMaster> GetStateMasterById(int StateId)
        {
            var statemaster = await _dbContext.StateMasters.FindAsync(StateId);
            if (statemaster != null)
            {
                return _mapper.Map<StateMaster>(statemaster);
            }
            return null;
        }
    }
}
