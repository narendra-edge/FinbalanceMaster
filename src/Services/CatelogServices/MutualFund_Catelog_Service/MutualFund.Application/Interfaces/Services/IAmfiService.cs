using MutualFund.Application.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Application.Interfaces.Services
{
    public interface IAmfiService
    {
        Task<List<AmfiSchemeDto>> GetAllSchemesAsync();
        Task<AmfiSchemeDto?> GetByCodeAsync(int Code);
    }
}
