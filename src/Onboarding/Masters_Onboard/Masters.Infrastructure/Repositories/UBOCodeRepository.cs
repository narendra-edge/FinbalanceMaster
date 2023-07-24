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
    public class UBOCodeRepository : IUBORepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public UBOCodeRepository(CatelogDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));   
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));    
        }
        public async Task<IEnumerable<UBOCode>> GetAllUBOCode()
        {
            var ubpcode = await _dbContext.UBOCodes.ToListAsync().ConfigureAwait(false);
            if (ubpcode != null)
            {
                return _mapper.Map<IEnumerable<UBOCode>>(ubpcode);
            }
            return null;
        }

        public async Task<UBOCode> GetUBOCodeById(int id)
        {
            var ubpcode = await _dbContext.UBOCodes.FindAsync(id);
            if (ubpcode != null)
            {
                return _mapper.Map<UBOCode>(ubpcode);
            }
            return null;
        }
    }
}
