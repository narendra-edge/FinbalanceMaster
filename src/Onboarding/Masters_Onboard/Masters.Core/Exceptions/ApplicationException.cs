using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixedIncomeInstrument.Core.Exceptions
{
    public class ApplicationException : Exception
    { 
        public ApplicationException() { }
        public ApplicationException(string message) : base(message) { }
        public ApplicationException(string message, params object[]args )
            :base(string.Format(CultureInfo.CurrentCulture, message,args))
        { }

    }
}
