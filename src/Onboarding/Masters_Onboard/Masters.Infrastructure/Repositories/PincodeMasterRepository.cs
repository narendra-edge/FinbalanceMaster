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
    public class PincodeMasterRepository : IPincodeMasterRepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public PincodeMasterRepository(CatelogDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));  
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<IEnumerable<PincodeMaster>> GetAllPinCodeMaster()
        {
            var pincodemaster = await _dbContext.PincodeMasters.Include(x => x.DistrictMaster).ToListAsync().ConfigureAwait(false);
            if (pincodemaster != null)
            {
                return _mapper.Map<IEnumerable<PincodeMaster>>(pincodemaster);
            }
            return null;
        }

        public async Task<PincodeMaster> GetPinCodeMasterById(int PinId)
        {
            var pincodemaster = await _dbContext.PincodeMasters.FindAsync(PinId);
            if (pincodemaster != null)
            {
                return _mapper.Map<PincodeMaster>(pincodemaster);
            }
            return null;
        }
    }
}
