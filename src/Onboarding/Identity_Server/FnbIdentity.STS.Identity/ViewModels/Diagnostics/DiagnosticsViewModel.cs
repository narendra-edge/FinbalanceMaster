using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace FnbIdentity.STS.Identity.ViewModels.Diagnostics
{
    public class DiagnosticsViewModel
    {
        public DiagnosticsViewModel(AuthenticateResult result)
        {
            AuthenticateResult = result;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (result.Properties.Items.ContainsKey("client_list"))
            {
                var encoded = result.Properties.Items["client_list"];
#pragma warning disable CS8604 // Possible null reference argument.
                var bytes = Base64Url.Decode(encoded);
#pragma warning restore CS8604 // Possible null reference argument.
                var value = Encoding.UTF8.GetString(bytes);

#pragma warning disable CS8601 // Possible null reference assignment.
                Clients = JsonConvert.DeserializeObject<string[]>(value);
#pragma warning restore CS8601 // Possible null reference assignment.
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public AuthenticateResult AuthenticateResult { get; }
        public IEnumerable<string> Clients { get; } = new List<string>();
    }
}
