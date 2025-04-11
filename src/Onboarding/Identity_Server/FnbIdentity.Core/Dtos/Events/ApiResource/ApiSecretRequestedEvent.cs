using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.ApiResource
{
    public class ApiSecretRequestedEvent
    {
        public int ApiResourceId { get; set; }

        public int ApiSecretId { get; set; }

        public string Type { get; set; }

        public DateTime? Expiration { get; set; }

        public ApiSecretRequestedEvent(int apiResourceId, int apiSecretId, string type, DateTime? expiration)
        {
            ApiResourceId = apiResourceId;
            ApiSecretId = apiSecretId;
            Type = type;
            Expiration = expiration;
        }
    }
}
