using Masters.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Interfaces.Repositories
{
    public interface IExchangeRepository 
    {
        Task<IEnumerable<Exchange>> GetAllExchanges();
        Task<Exchange> GetExchangeById(int ExchId);
        Task<Exchange> CreateExchange(Exchange exchange);
        Task<bool> UpdateExchange(int ExchId, Exchange exchange);
        Task<bool> DeleteExchange(int ExchId);
    }
}
