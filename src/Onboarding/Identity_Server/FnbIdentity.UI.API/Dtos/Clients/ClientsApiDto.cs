using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Dtos.Clients
{
    public class ClientsApiDto
    {
        public ClientsApiDto()
        {
            Clients = new List<ClientApiDto>();
        }

        public List<ClientApiDto> Clients { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }
    }
}
