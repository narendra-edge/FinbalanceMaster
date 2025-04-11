using FnbIdentity.Core.Dtos.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.ApiResource
{
    public class ApiResourcePropertiesRequestedEvent
    {
        public ApiResourcePropertiesRequestedEvent(int apiResourceId, ApiResourcePropertiesDto apiResourceProperties)
        {
            ApiResourceId = apiResourceId;
            ApiResourceProperties = apiResourceProperties;
        }

        public int ApiResourceId { get; set; }
        public ApiResourcePropertiesDto ApiResourceProperties { get; }
    }
}
