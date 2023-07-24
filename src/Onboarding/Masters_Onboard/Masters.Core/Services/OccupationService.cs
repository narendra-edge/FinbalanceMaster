using Masters.Core.Interfaces.Repositories;
using Masters.Core.Interfaces.Services;
using Masters.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Services
{
    public class OccupationService : IOccupationService
    {
        private readonly IOccupationRepository _occupation;
        private readonly ILogger<OccupationService> _logger;

        public OccupationService(IOccupationRepository occupation, ILogger<OccupationService> logger)
        {
            _occupation = occupation ?? throw new ArgumentNullException(nameof(occupation));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<Occupation>> GetAlOccupation()
        {
            try
            {
                return await _occupation.GetAllOccupation();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetAllOccupation in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<Occupation> GetOccupationById(int id)
        {
            try
            {
                return await _occupation.GetOccupationById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetOccupationById in service class, error message=(ex).");
                throw;
            }
        }
    }
}
