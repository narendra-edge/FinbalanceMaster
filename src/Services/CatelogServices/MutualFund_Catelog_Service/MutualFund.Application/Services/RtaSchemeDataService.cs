using AutoMapper;
using MutualFund.Application.DTOS;
using MutualFund.Application.Interfaces.Services;
using MutualFund.Core.Entities;
using MutualFund.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MutualFund.Application.Services
{
    public class RtaSchemeDataService : IRtaSchemeDataService
    {
        private readonly IRtaSchemeDataRepository _repository;
        private readonly IMapper _mapper;

        public RtaSchemeDataService(IRtaSchemeDataRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RtaSchemeDataDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<RtaSchemeDataDto>>(entities.ToList());
        }

        public async Task<RtaSchemeDataDto?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty) return null;
            var entity = await _repository.GetByIdAsync(id);
            return _mapper.Map<RtaSchemeDataDto?>(entity);
        }

        //public async Task<RtaSchemeDataDto?> GetByRtaSchemeCodeAsync(string rtaSchemeCode)
        //{
        //    if (string.IsNullOrWhiteSpace(rtaSchemeCode)) return null;
        //    var entity = await _repository.GetByRtaSchemeCodeAsync(rtaSchemeCode);
        //    return _mapper.Map<RtaSchemeDataDto?>(entity);
        //}

        public async Task AddAsync(RtaSchemeDataDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var entity = _mapper.Map<RtaSchemeData>(dto);
            await _repository.AddAsync(entity);
        }

        public async Task UpdateAsync(RtaSchemeDataDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var entity = _mapper.Map<RtaSchemeData>(dto);
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty) return;
            await _repository.DeleteAsync(id);
        }
    }
}
