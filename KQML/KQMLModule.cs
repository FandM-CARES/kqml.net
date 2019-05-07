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

        public string Host;
        public int Port;
        public bool IsApplication;
        public bool Testing;
        public Socket Socket;
        public string Name;
        public bool ScanForPort;
        public bool Debug;

        public KQMLModule()
        {
            Host = "localhost";
            Port = 6200;
            IsApplication = false;
            Testing = False;
            Socket = null;
            Debug = false;

            Dispatcher = null;
            MAX_PORT_TRIES = 100;
            ReplyIdCounter = 1;

            //TODO: Neds to handle command line argument and change defaults
        
            if(!Testing) {


            }                        
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
