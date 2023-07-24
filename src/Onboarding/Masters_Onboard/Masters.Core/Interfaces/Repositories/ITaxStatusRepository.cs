using Masters.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Interfaces.Repositories
{
    public interface ITaxStatusRepository
    {
        Task<IEnumerable<TaxStatus>> GetAllTaxStatus();
        Task<TaxStatus> GetTaxStatusById(int id);
    }
}
