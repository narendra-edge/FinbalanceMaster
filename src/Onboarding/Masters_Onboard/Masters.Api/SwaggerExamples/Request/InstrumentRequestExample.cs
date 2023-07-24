using Masters.Core.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Cryptography.X509Certificates;

namespace Masters.Api.SwaggerExamples.Request
{
    public class InstrumentRequestExample : IExamplesProvider<Instrument>
    {
        public Instrument GetExamples()
        {
            return new Instrument
            {
                InstrumentName = "PPF",
                InstrumentType = " Fixed Income",
                InstrumentIssuer = "IndiaPost", 
                Description = "",
                IsActive = true,
                Risk = "Credit Risk",
                ExchangeId = 1,
                CreatedBy = "Narendra Kumar",
                CreatedDate = DateTime.Now,
            };
        }
    }
}
