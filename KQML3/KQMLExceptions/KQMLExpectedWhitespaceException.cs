using System;
using System.Collections.Generic;
using System.Text;

namespace KQML.KQMLExceptions
{
    class KQMLExpectedWhitespaceException:Exception
    {
        public  KQMLExpectedWhitespaceException()
        {

        }

        public KQMLExpectedWhitespaceException(string message)
            : base(message)
        {

        }
    }
}
