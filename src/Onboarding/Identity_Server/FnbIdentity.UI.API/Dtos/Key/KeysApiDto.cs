using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.UI.API.Dtos.Key
{
    public class KeysApiDto
    {
        public KeysApiDto()
        {
            Keys = new List<KeyApiDto>();
        }
        public List<KeyApiDto> Keys { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
    }
}
