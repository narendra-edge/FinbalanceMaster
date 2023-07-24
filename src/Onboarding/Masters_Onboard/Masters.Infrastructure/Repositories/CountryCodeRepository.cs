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
    public class CountryCodeRepository : ICountryCodeRepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public CountryCodeRepository(CatelogDbContext dbContext, IMapper mapper)
        {
           _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext)); 
           _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<IEnumerable<CountryCode>> GetAllCountryCode()
        {
            var countrycodes = await _dbContext.CountryCodes.ToListAsync().ConfigureAwait(false);
            if (countrycodes != null)
            {
                return _mapper.Map<IEnumerable<CountryCode>>(countrycodes);
            }
            return null;
        }

        public async Task<CountryCode> GetCountryCodeById(int CtryId)
        {
            var countrycode = await _dbContext.CountryCodes.FindAsync(CtryId);
            if (countrycode != null)
            {
                return _mapper.Map<CountryCode>(countrycode);
            }
            return null;
        }
    }
}
