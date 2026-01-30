using MutualFund.Application.DTOS;
using MutualFund.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Application.Interfaces.Services
{
    public interface ISchemeMasterFinalService
    {
        Task<List<SchemeMasterFinalDto>> GetAllAsync();
        Task<SchemeMasterFinalDto?> GetByIdAsync(Guid id);
        Task AddAsync(SchemeMasterFinalDto dto);
        Task UpdateAsync(SchemeMasterFinalDto dto);
        Task DeleteAsync(Guid id);
       
    }
}
