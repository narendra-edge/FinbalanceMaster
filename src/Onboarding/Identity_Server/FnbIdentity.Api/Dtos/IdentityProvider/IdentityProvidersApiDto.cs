using System.Collections.Generic;

namespace FnbIdentity.Api.Dtos.IdentityProvider
{
    public class IdentityProvidersApiDto
    {
        public IdentityProvidersApiDto()
        {
            IdentityProviders = new List<IdentityProviderApiDto>();
        }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<IdentityProviderApiDto> IdentityProviders { get; set; }
    }
}
