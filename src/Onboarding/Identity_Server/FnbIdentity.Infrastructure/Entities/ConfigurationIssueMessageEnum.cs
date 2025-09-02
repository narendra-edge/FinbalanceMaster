using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Infrastructure.Entities
{
    public enum ConfigurationIssueMessageEnum
    {
        ObsoleteImplicitGrant,
        ObsoletePasswordGrant,
        MissingPkce,
    }
}
