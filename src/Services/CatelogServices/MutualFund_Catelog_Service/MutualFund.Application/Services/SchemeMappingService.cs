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
    public class SchemeMappingService : ISchemeMappingService
    {
        private readonly ISchemeMappingRepository _repository;
        private readonly IMapper _mapper;

        public SchemeMappingService(ISchemeMappingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<SchemeMappingDto>> GetAllMappingsAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<SchemeMappingDto>>(entities);
        }

        public async Task<SchemeMappingDto?> GetMappingByIdAsync(long id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<SchemeMappingDto?>(entity);
        }

        public async Task AddMappingAsync(SchemeMappingDto dto)
        {
            var entity = _mapper.Map<SchemeMapping>(dto);
            await _repository.AddAsync(entity);
        }

        public async Task UpdateMappingAsync(SchemeMappingDto dto)
        {
            var entity = _mapper.Map<SchemeMapping>(dto);
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteMappingAsync(long id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
