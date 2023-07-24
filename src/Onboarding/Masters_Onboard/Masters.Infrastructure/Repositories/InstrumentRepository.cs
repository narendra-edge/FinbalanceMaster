using AutoMapper;
using Masters.Core.Interfaces.Repositories;
using Masters.Core.Models;
using Masters.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Infrastructure.Repositories
{   
    public class InstrumentRepository : IInstrumentRepository
    {
        private readonly CatelogDbContext _dbcontext;

        private readonly IMapper _mapper;
        
        public InstrumentRepository(CatelogDbContext dbcontext, IMapper mapper)
        {
            _dbcontext = dbcontext ?? throw new ArgumentNullException(nameof(dbcontext));
            _mapper = mapper?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<Instrument> CreateInstrument(Instrument instrument)
        {
            var dbinsturment = _mapper.Map<Entities.Instrument>(instrument);
            await _dbcontext.AddAsync(dbinsturment);
            await _dbcontext.SaveChangesAsync();
            return instrument;
        }

        public async Task<bool> DeleteInstrument(int InstId)
        {
            var instrument = await _dbcontext.Instruments.FindAsync(InstId);
            if(instrument != null)
            {
                _dbcontext.Instruments.Remove(instrument);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Instrument>> GetAllInstruments()
        {
            var instruments = await _dbcontext.Instruments.Include(x => x.Exchange).ToListAsync().ConfigureAwait(false);
            if (instruments!=null)
            { 
                 return _mapper.Map<IEnumerable<Instrument>>(instruments);
            }
            return null;
        }

        public async Task<Instrument> GetInstrumentById(int InstId)
        {
            var Instrument = await _dbcontext.Instruments.FindAsync(InstId);
            if(Instrument!=null) 
            { 
               return _mapper.Map<Instrument>(Instrument);
            }
            return null;
        }

        public async Task<bool> UpdateInstrument(int InstId, Instrument instrument)
        {
            var dbinstrument = await _dbcontext.Instruments.FindAsync(InstId);
            if (dbinstrument != null  || dbinstrument.InstId != InstId)
            {
                return false;
            }
            dbinstrument.InstrumentName = instrument.InstrumentName;
            dbinstrument.InstrumentType = instrument.InstrumentType;
            dbinstrument.InstrumentIssuer = instrument.InstrumentIssuer;
            dbinstrument.Description = instrument.Description;
            dbinstrument.ExchangeId = instrument.ExchangeId;
            dbinstrument.IsActive = instrument.IsActive;
            dbinstrument.CreatedBy = instrument.CreatedBy;
            dbinstrument.CreatedDate = DateTime.Now;

            if(instrument != null)
            {
                _dbcontext.Instruments.Update(dbinstrument);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
