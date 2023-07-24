using Masters.Core.Interfaces.Repositories;
using Masters.Core.Interfaces.Services;
using Masters.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Services
{
    public class ExchangeService : IExchangeService
    {
        private readonly IExchangeRepository _exchangeRepository;
        private readonly ILogger<IExchangeRepository> _logger;

        public ExchangeService(IExchangeRepository exchangeRepository , ILogger<IExchangeRepository> logger)
        {
            _exchangeRepository = exchangeRepository;
            _logger = logger;
            
        }
        public async Task<Exchange> CreateExchange(Exchange exchange)
        {
            try
            {
                return await _exchangeRepository.CreateExchange(exchange);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call CreateExchange in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<bool> DeleteExchange(int ExchId)
        {
            try
            {
                return await _exchangeRepository.DeleteExchange(ExchId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call DeleteExchange in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<IEnumerable<Exchange>> GetAllExchanges()
        {

            try
            {
                return await _exchangeRepository.GetAllExchanges();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetAllExchanges in service class, error message=(ex).");
                throw;
            }
        }

        public async Task<Exchange> GetExchangeById(int ExchId)
        {
            try
            {
                return await _exchangeRepository.GetExchangeById(ExchId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call GetExchangeById in service class, error message=(ex).");

                throw;
            }
        }

        public async Task<bool> UpdateExchange(int ExchId, Exchange exchange)
        {
            try
            {
                return await _exchangeRepository.UpdateExchange(ExchId, exchange);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eooror while trying to call UpdateExchange in service class, error message=(ex).");

                throw;
            }
        }
    }
}
