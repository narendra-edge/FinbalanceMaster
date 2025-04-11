using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Dtos.Keys
{
    public class KeysDto
    {
        public KeysDto()
        {
            Keys = new List<KeyDto>();
        }

        public List<KeyDto> Keys { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }
    }
}
