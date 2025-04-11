using FnbIdentity.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FnbIdentity.Core.Resources
{
    public class PersistedGrantServiceResources : IPersistedGrantServiceResources
    {
        public virtual ResourceMessage PersistedGrantDoesNotExist()
        {
            return new ResourceMessage()
            {
                Code = nameof(PersistedGrantDoesNotExist),
                Description = PersistedGrantServiceResource.PersistedGrantDoesNotExist
            };
        }

        public virtual ResourceMessage PersistedGrantWithSubjectIdDoesNotExist()
        {
            return new ResourceMessage()
            {
                Code = nameof(PersistedGrantWithSubjectIdDoesNotExist),
                Description = PersistedGrantServiceResource.PersistedGrantWithSubjectIdDoesNotExist
            };
        }
    }
}
