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
    public class TaxStatusService : ITaxStatusSerice
    {
        private readonly ITaxStatusRepository _taxStatus;
        private readonly ILogger<TaxStatusService> _logger;

        public TaxStatusService(ITaxStatusRepository taxStatus, ILogger<TaxStatusService> logger)
        {
            _taxStatus = taxStatus ?? throw new ArgumentNullException(nameof(taxStatus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<TaxStatus>> GetAlTaxStatus()
        {
            try
            {
                return await _taxStatus.GetAllTaxStatus();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetAllTaxStatus in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<TaxStatus> GetTaxStatusById(int id)
        {
            try
            {
                return await _taxStatus.GetTaxStatusById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetTaxStatusById in service class, error message=(ex).");
                throw;
            }
        }
    }
}
