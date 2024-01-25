using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.Configuration
{
    public class DataProtectionConfiguration
    {
        public bool ProtectKeysWithAzureKeyVault { get; set; }
    }
}
