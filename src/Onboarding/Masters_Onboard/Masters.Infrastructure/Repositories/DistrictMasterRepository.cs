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
    public class DistrictMasterRepository : IDistrictMasterRepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public DistrictMasterRepository(CatelogDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<IEnumerable<DistrictMaster>> GetAllDistrictMaster()
        {
            var district = await _dbContext.DistrictMasters.Include(x => x.StateMaster).ToListAsync().ConfigureAwait(false);
            if (district != null)
            {
                return _mapper.Map<IEnumerable<DistrictMaster>>(district);
            }
            return null;
        }

        public async Task<DistrictMaster> GetDistrictMasterById(int DstrId)
        {
            var district = await _dbContext.DistrictMasters.FindAsync(DstrId);
            if (district != null)
            {
                return _mapper.Map<DistrictMaster>(district);
            }
            return null;
        }
    }
}
