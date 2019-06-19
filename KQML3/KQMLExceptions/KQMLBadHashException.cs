using System;
using System.Collections.Generic;
using System.Text;

namespace KQML.KQMLExceptions
{
    class KQMLBadHashException : Exception
    {
        public KQMLBadHashException()
        {

        }

        public KQMLBadHashException(string message)
            : base(message)
        {

        }
    }
}
