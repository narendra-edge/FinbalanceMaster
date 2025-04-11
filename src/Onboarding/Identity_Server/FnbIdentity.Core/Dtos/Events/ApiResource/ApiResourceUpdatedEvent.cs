using FnbIdentity.Core.Dtos.Configuration;


namespace FnbIdentity.Core.Dtos.Events.ApiResource
{
    public class ApiResourceUpdatedEvent
    {
        public ApiResourceDto OriginalApiResource { get; set; }
        public ApiResourceDto ApiResource { get; set; }

        public ApiResourceUpdatedEvent(ApiResourceDto originalApiResource, ApiResourceDto apiResource)
        {
            OriginalApiResource = originalApiResource;
            ApiResource = apiResource;
        }
    }
}
