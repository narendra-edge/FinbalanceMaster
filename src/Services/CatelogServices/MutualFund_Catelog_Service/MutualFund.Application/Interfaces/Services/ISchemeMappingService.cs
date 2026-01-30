using MutualFund.Application.DTOS;
using MutualFund.Application.Services;
using MutualFund.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Application.Interfaces.Services
{
    public interface ISchemeMappingService
    {
        Task<List<SchemeMappingDto>> GetAllMappingsAsync();
        Task<SchemeMappingDto?> GetMappingByIdAsync(long id);
        Task AddMappingAsync(SchemeMappingDto dto);
        Task UpdateMappingAsync(SchemeMappingDto dto);
        Task DeleteMappingAsync(long id);
    }
}
