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
    public class SourceOfWealthService : ISourceOfWealthService
    {
        private readonly ISourceOfWealthRepository _sourceOfwealth;
        private readonly ILogger<ISourceOfWealthRepository> _logger;

        public SourceOfWealthService(ISourceOfWealthRepository sourceOfWealth, ILogger<ISourceOfWealthRepository> logger)
        {
            _sourceOfwealth = sourceOfWealth ?? throw new ArgumentNullException(nameof(sourceOfWealth));    
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<SourceOfWealth>> GetAlSourceOfWealth()
        {
            try
            {
                return await _sourceOfwealth.GetAllSourceOfWealth();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetAlSourceOfWealth in service class, error message=(ex).");
                throw;
            }
            
        }

        public async Task<SourceOfWealth> GetSourceOfWealthById(int SrcId)
        {
            try
            {
                return await _sourceOfwealth.GetSourceOfWealthById(SrcId);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetSourceOfWealthById in service class, error message=(ex).");
                throw;
            }
        }
    }
}
