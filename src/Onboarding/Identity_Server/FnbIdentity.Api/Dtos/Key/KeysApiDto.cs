using System.Collections.Generic;

namespace FnbIdentity.Api.Dtos.Key
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
