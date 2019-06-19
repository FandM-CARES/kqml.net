using System;
using System.Collections.Generic;
using System.Text;

namespace KQML
{
    class KQMLBadCloseException : Exception
    {
        public KQMLBadCloseException()
        {

        }

        public KQMLBadCloseException(string message)
            : base(message)
        {

        }
    }
}
