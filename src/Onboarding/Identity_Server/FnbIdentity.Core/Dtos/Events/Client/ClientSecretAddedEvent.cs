using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Events.Client
{
    public class ClientSecretAddedEvent
    {
        public string Type { get; set; }

        public DateTime? Expiration { get; set; }

        public int ClientId { get; set; }

        public ClientSecretAddedEvent(int clientId, string type, DateTime? expiration)
        {
            ClientId = clientId;
            Type = type;
            Expiration = expiration;
        }
    }
}
