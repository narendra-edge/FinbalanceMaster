using Masters.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Interfaces.Services
{
    public interface ICountryCodeService
    {
        Task<IEnumerable<CountryCode>> GetAlCountryCode();
        Task<CountryCode> GetCountryCodeById(int CtryId);
    }
}
