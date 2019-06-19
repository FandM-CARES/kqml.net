using System;
using System.Collections.Generic;
using System.Text;

namespace KQML.KQMLExceptions
{
    class KQMLBadCommandException:Exception
    {
        public KQMLBadCommandException()
        {

        }

        public KQMLBadCommandException(string message)
            : base(message)
        {

        }
    }
}
