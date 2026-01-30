using AutoMapper;
using MutualFund.Application.DTOS;
using MutualFund.Application.Interfaces.Services;
using MutualFund.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MutualFund.Application.Services
{
    public class AmfiService : IAmfiService
    {
        private readonly IAmfiSchemeRepository _repo;
        private readonly IMapper _mapper;

        public AmfiService(IAmfiSchemeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<AmfiSchemeDto>> GetAllSchemesAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<List<AmfiSchemeDto>>(entities);
        }

        public async Task<AmfiSchemeDto?> GetByCodeAsync(int code)
        {
            var entity = await _repo.GetByCodeAsync(code);
            return _mapper.Map<AmfiSchemeDto?>(entity);
        }
    }
}
