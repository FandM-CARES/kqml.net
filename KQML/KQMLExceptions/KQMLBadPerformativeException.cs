using System;
using System.Collections.Generic;
using System.Text;

namespace KQML.KQMLExceptions
{
    public class KQMLBadPerformativeException : Exception
    {
        public KQMLBadPerformativeException()
        {

        }

        public KQMLBadPerformativeException(string message)
            : base(message)
        {

        }
    }
}
