using System;
using System.Collections.Generic;
using System.Text;

namespace KQML.KQMLExceptions
{
    class KQMLExpectedListException:Exception
    {
        public KQMLExpectedListException()
        {

        }

        public KQMLExpectedListException(string message)
            : base(message)
        {

        }
    }
}
