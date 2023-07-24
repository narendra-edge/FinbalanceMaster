using Masters.Core.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Masters.Api.SwaggerExamples.Response
{
    public class InstrumentResponseExample : IExamplesProvider<Instrument>
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
