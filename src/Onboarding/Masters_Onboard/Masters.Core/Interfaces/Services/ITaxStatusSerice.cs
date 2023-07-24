using Masters.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Interfaces.Services
{
    public interface ITaxStatusSerice
    {
        Task<IEnumerable<TaxStatus>> GetAlTaxStatus();
        Task<TaxStatus> GetTaxStatusById(int id);
    }
}
