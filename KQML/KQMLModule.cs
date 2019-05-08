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
        public Socket Socket;
        public string Name;
        public bool ScanForPort;
        public bool Debug;
        public StreamReader Out;
        public KQMLReader Inp;
        public string GroupName;

        public KQMLModule()
        {
            Host = "localhost";
            Port = 6200;
            IsApplication = false;
            Testing = false;
            Socket = null;
            Debug = false;

            Dispatcher = null;
            MAX_PORT_TRIES = 100;
            ReplyIdCounter = 1;

            //TODO: Needs to handle command line argument and change defaults
        
            if(!Testing)
            {
                Out = null;
                Inp = null;
                //TODO: Log here
                bool conn = Connect(Host, Port);
                if(!conn)
                {
                    //TODO: Log error
                    Exit(-1);
                }
                //UNDONE: use if to check for assert invariant
               
               

            }
            else
            {
                //TODO: Using stdio connection
                //UNDONE: what is this bytes stuff doing
                //Maybe not put test code here at all 
            }
            Dispatcher = new KQMLDispatcher(this, Inp, Name);
            Register();
        }

        private void Exit(int v)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            if (!Testing)
                Dispatcher.Start();
        }

        public void SubscribeRequest(string reqType)
        {
            KQMLPerformative msg= new KQMLPerformative("subscribe");
            KQMLList content = new KQMLList("request)");
            content.Append("&key");
            content.Set("content", KQMLList.FromString($"({reqType} . *)"));
            msg.Set("content", content);
            Send(msg);
        }

        public void SubscribeTell(string tellType)
        {
            KQMLPerformative msg = new KQMLPerformative("subscribe");
            KQMLList content = new KQMLList("tell");
            content.Append("&key");
            content.Set("content", KQMLList.FromString($"({tellType} . *)"));
            msg.Set("content", content);
            Send(msg);
        }
        public bool Connect(string host = null, int startPort = 0)
        {
            if (string.IsNullOrEmpty(host))
                host = Host;
            if (startPort == 0)
                startPort = Port;
            if (!ScanForPort)
                return Connect1(host, startPort, true);
            else
            {
                int maxtries = MAX_PORT_TRIES;
                for (int port = startPort; port < startPort + MAX_PORT_TRIES; port++)
                {
                    bool conn = Connect1(host, port, false);
                    if (conn)
                        return true;

                }
                //TODO: Log error: failed to connect
                return false;
            }
        }

        private bool Connect1(string host, int port, bool verbose)
        {
            try
            {
                //Crreate Socket
                Socket.Connect(host, port);
                // TODO: Socket magic!
                return true;

            }
            catch (SocketException e)
            {
                if(verbose)
                {
                    //Log error with e
                }
                return false;
            }
        }

        private void Send(KQMLPerformative msg)
        {
            throw new NotImplementedException();
        }

        private void Register()
        {
            if(!string.IsNullOrEmpty(Name))
            {
                KQMLPerformative perf = new KQMLPerformative("register");
                perf.Set("name", Name);
                if (string.IsNullOrEmpty(GroupName))
                {

                }
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
