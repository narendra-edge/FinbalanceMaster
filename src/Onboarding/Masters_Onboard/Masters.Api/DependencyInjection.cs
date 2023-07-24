using Masters.Api.Helper;
using Masters.Api.Middleware;
using Masters.Core.Interfaces.Repositories;
using Masters.Core.Interfaces.Services;
using Masters.Core.Services;
using Masters.Infrastructure.Context;
using Masters.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;


namespace Masters.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            
            //services.AddDbContext<CatelogDbContext>(opt => opt.UseInMemoryDatabase("InMem"));          
            services.AddAutoMapper(typeof(MapperProfile));           
           
            services.AddScoped<ICountryCodeRepository, CountryCodeRepository>();
            services.AddScoped<ICountryCodeService, CountryCodeService>();
            services.AddScoped<IDistrictMasterRepository, DistrictMasterRepository>();
            services.AddScoped<IDistrictMasterService, DistrictMasterService>();
            services.AddScoped<IExchangeRepository, ExchangeRepository>();
            services.AddScoped<IExchangeService, ExchangeService>();          
            services.AddScoped<IIncomeDetailRepository, IncomeDetailRepository>();
            services.AddScoped<IIncomeDetailService, IncomeDetailService>();
            services.AddScoped<IInstrumentRepository, InstrumentRepository>();
            services.AddScoped<IInstrumentService, InstrumentService>();
            services.AddScoped<IOccupationRepository, OccupationRepository>();
            services.AddScoped<IOccupationService, OccupationService>();
            services.AddScoped<IPincodeMasterRepository, PincodeMasterRepository>();
            services.AddScoped<IPincodeMasterService, PincodeMasterService>();
            services.AddScoped<ISourceOfWealthRepository, SourceOfWealthRepository>();
            services.AddScoped<ISourceOfWealthService, SourceOfWealthService>();
            services.AddScoped<IStateMasterRepository, StateMasterRepository>();
            services.AddScoped<IStateMasterService, StateMasterService>();           
            services.AddScoped<ITaxStatusRepository, TaxStatusRepository>();
            services.AddScoped<ITaxStatusSerice, TaxStatusService>();          
            services.AddScoped<IUBORepository, UBOCodeRepository>();
            services.AddScoped<IUBOService, UBOCodeService>();
            

            services.ConfigureCors();
            services.AddRouting(options => options.LowercaseUrls= true);
            services.AddHttpClient();
            return services;
        }
    }
}
