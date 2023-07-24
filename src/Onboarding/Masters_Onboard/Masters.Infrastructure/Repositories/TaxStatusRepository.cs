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
    public class TaxStatusRepository : ITaxStatusRepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public TaxStatusRepository(CatelogDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));    
        }
        public async Task<IEnumerable<TaxStatus>> GetAllTaxStatus()
        {
            var taxstatus = await _dbContext.TaxStatuses.ToListAsync().ConfigureAwait(false);
            if (taxstatus != null)
            {
                return _mapper.Map<IEnumerable<TaxStatus>>(taxstatus);
            }
            return null;
        }

        public async Task<TaxStatus> GetTaxStatusById(int id)
        {
            var taxstatus = await _dbContext.TaxStatuses.FindAsync(id);
            if (taxstatus != null)
            {
                return _mapper.Map<TaxStatus>(taxstatus);
            }
            return null;
        }
    }
}
