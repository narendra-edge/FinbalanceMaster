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
    public class StateMasterService : IStateMasterService
    {
        private readonly IStateMasterRepository _stateMaster;
        private readonly ILogger<IStateMasterRepository> _logger;

        public StateMasterService(IStateMasterRepository stateMaster, ILogger<IStateMasterRepository> logger)
        {
            _stateMaster = stateMaster ?? throw new ArgumentNullException(nameof(stateMaster));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<StateMaster>> GetAllStateMaster()
        {
            try
            {
                return await _stateMaster.GetAllStateMaster();
            }
            catch (Exception ex)
            {

                _logger.LogError($"Eooror while trying to call GetAllStateMaster in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<StateMaster> GetStateMasterById(int StateId)
        {
            try
            {
                return await _stateMaster.GetStateMasterById(StateId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetStateMasterById in service class, error message=(ex).");
                throw;
            }
        }
    }
}
