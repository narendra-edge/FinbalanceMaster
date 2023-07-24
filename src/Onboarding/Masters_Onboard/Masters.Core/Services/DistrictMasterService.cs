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
    public  class DistrictMasterService : IDistrictMasterService
    {
        private readonly IDistrictMasterRepository _districtMaster;
        private readonly ILogger<IDistrictMasterRepository> _logger;

        public DistrictMasterService(IDistrictMasterRepository districtMaster, ILogger<IDistrictMasterRepository> logger)
        {
            _districtMaster = districtMaster ?? throw new ArgumentNullException(nameof(districtMaster));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DistrictMaster> GetDistrictMasterById(int DstrId)
        {
            try
            {
                return await _districtMaster.GetDistrictMasterById(DstrId);  
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetDistrictMasterById in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<IEnumerable<DistrictMaster>> GetAllDistrictMaster()
        {
            try
            {
                return await _districtMaster.GetAllDistrictMaster();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetAllDistrictMaster in service class, error message=(ex).");
                throw;
            }
        }
    }
}
