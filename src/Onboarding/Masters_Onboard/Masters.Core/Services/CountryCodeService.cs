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
    public class CountryCodeService : ICountryCodeService
    {
        private readonly ICountryCodeRepository _countryCode;
        private readonly ILogger<ICountryCodeService> _logger;

        public CountryCodeService(ICountryCodeRepository countryCode, ILogger<ICountryCodeService> logger)
        {
            _countryCode = countryCode ?? throw new ArgumentNullException(nameof(countryCode)); 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<CountryCode>> GetAlCountryCode()
        {
            try
            {
                return await _countryCode.GetAllCountryCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetAllCountryCode in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<CountryCode> GetCountryCodeById(int CtryId)
        {
            try
            {
                return await _countryCode.GetCountryCodeById(CtryId);
            }
            catch (Exception)
            {
                _logger.LogError($"Eooror while trying to call GetCountryCodeById in service class, error message=(ex).");
                throw;
            }
        }
    }
}
