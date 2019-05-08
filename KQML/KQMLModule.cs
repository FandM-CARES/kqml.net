using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
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
        //public Socket Socket;
        public string Name;
        public bool ScanForPort;
        public bool Debug;

        public StreamReader In;
        public StreamWriter Out;

        public KQMLModule()
        {
            Host = "localhost";
            Port = 6200;
            IsApplication = false;
            Testing = false;
            //Socket = null;
            Debug = false;

            Dispatcher = null;
            MAX_PORT_TRIES = 100;
            ReplyIdCounter = 1;

            //TODO: Neds to handle command line argument and change defaults
            // JRW: Worry about that later

            connect(Host, Port);
                                   
        }

        private void connect(string host, int port)
        {
            TcpClient client = new TcpClient(host, port);
            NetworkStream ns = client.GetStream();
            In = new StreamReader(ns);
            Out = new StreamWriter(ns);
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
