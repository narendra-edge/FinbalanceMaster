using MutualFund.Application.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Application.Interfaces.Services
{
    public interface IRtaSchemeDataService
    {
        Task<IEnumerable<RtaSchemeDataDto>> GetAllAsync();
        Task<RtaSchemeDataDto?> GetByIdAsync(Guid id);
        Task AddAsync(RtaSchemeDataDto dto);
        Task UpdateAsync(RtaSchemeDataDto dto);
        Task DeleteAsync(Guid id);
    }
}
