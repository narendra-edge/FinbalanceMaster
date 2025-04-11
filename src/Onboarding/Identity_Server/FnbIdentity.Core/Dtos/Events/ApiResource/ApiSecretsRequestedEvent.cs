using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.ApiResource
{
    public class ApiSecretsRequestedEvent
    {
        public int ApiResourceId { get; set; }

        public List<(int apiSecretId, string type, DateTime? expiration)> Secrets { get; set; }


        public ApiSecretsRequestedEvent(int apiResourceId, List<(int apiSecretId, string type, DateTime? expiration)> secrets)
        {
            ApiResourceId = apiResourceId;
            Secrets = secrets;
        }
    }
}
