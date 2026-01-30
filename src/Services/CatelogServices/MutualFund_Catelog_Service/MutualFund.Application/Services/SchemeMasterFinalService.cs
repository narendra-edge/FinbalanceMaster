using AutoMapper;
using MutualFund.Application.DTOS;
using MutualFund.Application.Interfaces.Services;
using MutualFund.Core.Entities;
using MutualFund.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualFund.Application.Services
{
    public class SchemeMasterFinalService : ISchemeMasterFinalService
    {
        private readonly ISchemeMasterFinalRepository _repository;
        private readonly IMapper _mapper;

        public SchemeMasterFinalService(ISchemeMasterFinalRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<SchemeMasterFinalDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<SchemeMasterFinalDto>>(entities);
        }

        public async Task<SchemeMasterFinalDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<SchemeMasterFinalDto?>(entity);
        }
        
        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task AddAsync(SchemeMasterFinalDto dto)
        {
            var entity = _mapper.Map<SchemeMasterFinal>(dto);
            await _repository.AddAsync(entity);
        }

        public async Task UpdateAsync(SchemeMasterFinalDto dto)
        {
            var entity = _mapper.Map<SchemeMasterFinal>(dto);
            await _repository.UpdateAsync(entity);
        }
    }
}
