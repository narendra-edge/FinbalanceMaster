using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Configuration
{
    public class ClientsDto
    {
        public ClientsDto()
        {
            Clients = new List<ClientDto>();
        }

        public List<ClientDto> Clients { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }
    }
}
