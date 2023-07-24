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
    public class IncomeDetailService : IIncomeDetailService
    {
        private readonly IIncomeDetailRepository _incomeDetail;
        private readonly ILogger<IncomeDetailService> _logger;

        public IncomeDetailService(IIncomeDetailRepository incomeDetail, ILogger<IncomeDetailService> logger)
        {
            _incomeDetail = incomeDetail ?? throw new ArgumentNullException(nameof(incomeDetail));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<IncomeDetail>> GetAlIncomeDetail()
        {
            try
            {
                return await _incomeDetail.GetAlIncomeDetail();
            }
            catch (Exception)
            {

                _logger.LogError($"Eooror while trying to call GetAlIncomeDetail in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<IncomeDetail> GetIncomeDetailById(int id)
        {
            try
            {
                return await _incomeDetail.GetIncomeDetailById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetIncomeDetailById in service class, error message=(ex).");
                throw;
            }
        }
    }
}
