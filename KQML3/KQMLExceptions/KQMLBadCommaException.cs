using System;
using System.Collections.Generic;
using System.Text;

namespace KQML.KQMLExceptions
{
    class KQMLBadCommaException : Exception
    {
        public KQMLBadCommaException()
        {

        }

        public KQMLBadCommaException(string message)
            : base(message)
        {

        }
    }
}
