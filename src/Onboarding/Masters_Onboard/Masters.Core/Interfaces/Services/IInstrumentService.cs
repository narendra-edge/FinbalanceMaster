using Masters.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Interfaces.Services
{
    public interface IInstrumentService
    {
        
        Task<IEnumerable<Instrument>> GetAllInstruments();
        Task<Instrument> GetInstrumentById(int InstId);
        Task<Instrument> CreateInstrument(Instrument instrument);
        Task<bool> UpdateInstrument(int InstId, Instrument instrument);
        Task<bool> DeleteInstrument(int InstId);
    }
}
