using FnbIdentity.Core.Dtos.Configuration;


namespace FnbIdentity.Core.Dtos.Events.ApiResource
{
    public class ApiResourcesRequestedEvent
    {
        public ApiResourcesRequestedEvent(ApiResourcesDto apiResources)
        {
            ApiResources = apiResources;
        }

        public ApiResourcesDto ApiResources { get; set; }
    }
}
