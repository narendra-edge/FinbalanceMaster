using Masters.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Interfaces.Repositories
{
    public interface IIncomeDetailRepository
    {
        Task<IEnumerable<IncomeDetail>> GetAlIncomeDetail();
        Task<IncomeDetail> GetIncomeDetailById(int id);
    }
}
