using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KQML
{
    class KQMLModule
    {
        public Dictionary<object, object> Defaults;
        public KQMLDispatcher Dispatcher;
        public int MAX_PORT_TRIES;
        public int ReplyIdCounter;

        public KQMLModule()
        {

        }

        public void ReceiveEof()
        {
            throw new NotImplementedException();
        }

        public void HandleException(IOException e)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessageMissingVerb(KQMLObject msg)
        {
            throw new NotImplementedException();
        }
        
        public void ReceiveMessageMissingContent(KQMLObject msg)
        {
            throw new NotImplementedException();
        }

        internal void ReceiveOtherPerformative(KQMLObject msg)
        {
            throw new NotImplementedException();
        }
    }
}
