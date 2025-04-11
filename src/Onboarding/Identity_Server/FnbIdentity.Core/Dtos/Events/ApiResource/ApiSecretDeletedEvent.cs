using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.ApiResource
{
    public class ApiSecretDeletedEvent
    {
        public int ApiResourceId { get; set; }

        public int ApiSecretId { get; set; }

        public ApiSecretDeletedEvent(int apiResourceId, int apiSecretId)
        {
            ApiResourceId = apiResourceId;
            ApiSecretId = apiSecretId;
        }
    }
}
