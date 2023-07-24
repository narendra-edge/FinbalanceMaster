using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Exceptions
{
    [Serializable]
    public  class NotFoundException : Exception
    {
        public NotFoundException()
        {
        }
        public NotFoundException(string message) : base(message) 
        {
        }
        public NotFoundException(string message,Exception inner) 
            : base(message, inner) 
        {
        }
        protected NotFoundException(
            SerializationInfo info, StreamingContext context )
            : base(info, context )
        { 
        }
    }
}
