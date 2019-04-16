using System;
using System.Collections.Generic;
using System.Text;

namespace KQML
{
    class KQMLBadCharacterException: Exception
    {
        public KQMLBadCharacterException()
        {

        }

        public KQMLBadCharacterException (string message)
            :base(message)
        {
    
        }
    }
}
