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
    public class PincodeMasterService : IPincodeMasterService
    {
        private readonly IPincodeMasterRepository _pincodeMaster;
        private readonly ILogger<IPincodeMasterRepository> _logger;

        public PincodeMasterService(IPincodeMasterRepository pincodeMaster, ILogger<IPincodeMasterRepository> logger)
        {
            _pincodeMaster = pincodeMaster ?? throw new ArgumentNullException(nameof(pincodeMaster));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<PincodeMaster>> GetAllPinCodeMaster()
        {
            try
            {
                return await _pincodeMaster.GetAllPinCodeMaster();
            }
            catch (Exception ex)
            {

                _logger.LogError($"Eooror while trying to call GetAllpincodemaster in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<PincodeMaster> GetPinCodeMasterById(int PinId)
        {
            try
            {
                return await _pincodeMaster.GetPinCodeMasterById(PinId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetPinCodeMasterById in service class, error message=(ex).");
                throw;
            }
        }
    }
}
