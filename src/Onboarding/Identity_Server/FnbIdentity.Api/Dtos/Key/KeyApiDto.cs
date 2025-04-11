using System;

namespace FnbIdentity.Api.Dtos.Key
{
    public class KeyApiDto
    {
        public string Id { get; set; }
        public int Version { get; set; }
        public DateTime Created { get; set; }
        public string Use { get; set; }
        public string Algorithm { get; set; }
        public bool IsX509Certificate { get; set; }
    }
}
