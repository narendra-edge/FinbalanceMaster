using MutualFund.Application.DTOS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MutualFund.Application.Interfaces.Services
{
    public interface IRtaSchemeDataService
    {
        Task<List<RtaSchemeDataDto>> GetAllAsync();
        Task<RtaSchemeDataDto?> GetByIdAsync(Guid id);
        Task<RtaSchemeDataDto?> GetByRtaSchemeCodeAsync(string rtaSchemeCode);
        Task AddAsync(RtaSchemeDataDto dto);
        Task UpdateAsync(RtaSchemeDataDto dto);
        Task DeleteAsync(Guid id);
    }
}