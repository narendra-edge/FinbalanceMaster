using Masters.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Interfaces.Services
{
    public interface IUBOService
    {
        Task<IEnumerable<UBOCode>> GetAlUBOCode();
        Task<UBOCode> GetUBOCodeById(int id);
    }
}
