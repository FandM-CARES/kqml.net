using System;
using System.Collections.Generic;
using System.Text;

namespace KQML.KQMLExceptions
{
    class KQMLBadOpenException : Exception
    {
        public KQMLBadOpenException()
        {

        }

        public KQMLBadOpenException(string message)
            : base(message)
        {

        }
    }
}
