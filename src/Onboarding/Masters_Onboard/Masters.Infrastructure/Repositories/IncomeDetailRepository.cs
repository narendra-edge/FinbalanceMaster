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
    public class IncomeDetailRepository : IIncomeDetailRepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public IncomeDetailRepository(CatelogDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException (nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException (nameof (mapper));
        }
        public async Task<IEnumerable<IncomeDetail>> GetAlIncomeDetail()
        {
            var incomeDetail = await _dbContext.IncomeDetails.ToListAsync().ConfigureAwait(false);
            if (incomeDetail != null)
            {
                return _mapper.Map<IEnumerable<IncomeDetail>>(incomeDetail);
            }
            return null;
        }

        public async Task<IncomeDetail> GetIncomeDetailById(int id)
        {
            var incomeDetail = await _dbContext.IncomeDetails.FindAsync(id);
            if (incomeDetail != null)
            {
                return _mapper.Map<IncomeDetail>(incomeDetail);
            }
            return null;
        }
    }
}
