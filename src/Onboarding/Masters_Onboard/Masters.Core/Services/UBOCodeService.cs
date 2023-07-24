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
    public class UBOCodeService : IUBOService
    {
        private readonly IUBORepository _uBOCode;
        private readonly ILogger<UBOCodeService> _logger;

        public UBOCodeService(IUBORepository uBOCode, ILogger<UBOCodeService> logger)
        {
            _uBOCode = uBOCode ?? throw new ArgumentNullException(nameof(uBOCode)); 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<UBOCode>> GetAlUBOCode()
        {
            try
            {
                return await _uBOCode.GetAllUBOCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetAllUBOCode in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<UBOCode> GetUBOCodeById(int id)
        {
            try
            {
                return await _uBOCode.GetUBOCodeById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetUBOCodeById in service class, error message=(ex).");
                throw;
            }
        }
    }
}
