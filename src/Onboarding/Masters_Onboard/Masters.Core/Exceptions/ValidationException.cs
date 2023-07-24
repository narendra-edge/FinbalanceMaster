using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Masters.Core.Exceptions
{
    [Serializable]
    public  class ValidationException : DomainException
    {
        public ValidationException() 
        {
        }
        public ValidationException(string message) : base(message)
        { 
        }
        public ValidationException(string message, Exception inner) 
            : base(message, inner)
        {

        }
        protected ValidationException(
            SerializationInfo info, StreamingContext context )
            :base(info, context ) { }
    }
}
