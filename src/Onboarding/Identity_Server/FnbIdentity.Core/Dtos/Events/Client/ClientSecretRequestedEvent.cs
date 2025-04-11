using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.Client
{
    public class ClientSecretRequestedEvent
    {

        public int ClientId { get; set; }

        public int ClientSecretId { get; set; }

        public string Type { get; set; }

        public DateTime? Expiration { get; set; }

        public ClientSecretRequestedEvent(int clientId, int clientSecretId, string type, DateTime? expiration)
        {
            ClientId = clientId;
            ClientSecretId = clientSecretId;
            Type = type;
            Expiration = expiration;
        }
    }
}
