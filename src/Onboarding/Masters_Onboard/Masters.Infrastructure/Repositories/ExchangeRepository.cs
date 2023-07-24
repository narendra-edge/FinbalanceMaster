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
    public class ExchangeRepository : IExchangeRepository
    {
        private readonly CatelogDbContext _dbContext;
        private readonly IMapper _mapper;

        public ExchangeRepository(CatelogDbContext dbContext , IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            
        }
        public async  Task<Exchange> CreateExchange(Exchange exchange)
        {
            var dbexchange = _mapper.Map<Entities.Exchange>(exchange);
             _dbContext.Add(dbexchange);
            await _dbContext.SaveChangesAsync();
            return exchange;
        }

        public async Task<bool> DeleteExchange(int ExchId)
        {
            var exchange = await _dbContext.Exchanges.FindAsync(ExchId);
            if (exchange != null)
            {
                _dbContext.Exchanges.Remove(exchange);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Exchange>> GetAllExchanges()
        {
            var exchanges = await _dbContext.Exchanges.ToListAsync().ConfigureAwait(false);
            if (exchanges != null)
            {
                return _mapper.Map<IEnumerable<Exchange>>(exchanges);
            }
            return null;
        }

        public async Task<Exchange> GetExchangeById(int ExchId)
        {
            var Exchange = await _dbContext.Exchanges.FindAsync(ExchId);
            if (Exchange != null)
            {
                return _mapper.Map<Exchange>(Exchange);
            }
            return null;
        }

        public async Task<bool> UpdateExchange(int ExchId, Exchange exchange)
        {
            var dbexchange = await _dbContext.Exchanges.FindAsync(ExchId);
            if (dbexchange != null || dbexchange.ExchId != ExchId)
            {
                return false;
            }
            dbexchange.ExchangeName = exchange.ExchangeName;
            dbexchange.Description = exchange.Description;
            

            if (exchange != null)
            {
                _dbContext.Exchanges.Update(dbexchange);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
