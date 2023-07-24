using Masters.Core.Interfaces.Repositories;
using Masters.Core.Interfaces.Services;
using Masters.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Services
{
    public class InstrumentService : IInstrumentService
    {
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly ILogger<IInstrumentRepository> _logger;
        public InstrumentService(IInstrumentRepository instrumentRepository, ILogger<IInstrumentRepository> logger)
        {
            _instrumentRepository = instrumentRepository ?? throw new ArgumentNullException(nameof(instrumentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(_logger));
        }
        public async Task<Instrument> CreateInstrument(Instrument instrument)
        {
            try
            {
                return await _instrumentRepository.CreateInstrument(instrument);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call CreateInstrument in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<bool> DeleteInstrument(int InstId)
        {
            try
            {
                return await _instrumentRepository.DeleteInstrument(InstId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call DeleteInstrument in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<IEnumerable<Instrument>> GetAllInstruments()
        {
            try
            {
                return await _instrumentRepository.GetAllInstruments();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetAllInstrument in service class, error message=(ex).");
                throw;
            }


        }

        public async Task<Instrument> GetInstrumentById(int InstId)
        {
            try
            {
                return await _instrumentRepository.GetInstrumentById(InstId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetInstrumentById in service class, error message=(ex).");

                throw;
            }
        }

        public async Task<bool> UpdateInstrument(int InstId, Instrument instrument)
        {
            try
            {
                return await _instrumentRepository.UpdateInstrument(InstId, instrument);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call UpdateInstrument in service class, error message=(ex).");

                throw;
            }
        }
    }
}
