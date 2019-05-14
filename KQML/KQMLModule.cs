using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace KQML
{
    class KQMLModule
    {
        public Dictionary<object, object> aults;

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
        public string GroupName;

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

            //TODO: Neds to handle command line argument and change aults
            // JRW: Worry about that later

            Connect(Host, Port);

        }

        private void Connect(string host, int port)
        {
            TcpClient client = new TcpClient(host, port);
            NetworkStream ns = client.GetStream();
            In = new StreamReader(ns);
            Out = new StreamWriter(ns);
        }

        public void Start()
        {
            if (!Testing)
                Dispatcher.Start();
        }

        public void SubscribeRequest(string reqType)
        {
            KQMLPerformative msg = new KQMLPerformative("subscribe");
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
        //public bool Connect(string host = null, int startPort = 0)
        //{
        //    if (string.IsNullOrEmpty(host))
        //        host = Host;
        //    if (startPort == 0)
        //        startPort = Port;
        //    if (!ScanForPort)
        //        return Connect1(host, startPort, true);
        //    else
        //    {
        //        int maxtries = MAX_PORT_TRIES;
        //        for (int port = startPort; port < startPort + MAX_PORT_TRIES; port++)
        //        {
        //            bool conn = Connect1(host, port, false);
        //            if (conn)
        //                return true;

        //        }
        //        //TODO: Log error: failed to connect
        //        return false;
        //    }
        //}

        //private bool Connect1(string host, int port, bool verbose)
        //{
        //    try
        //    {
        //        //Crreate Socket
        //        Socket.Connect(host, port);
        //        // TODO: Socket magic!
        //        return true;

        //    }
        //    catch (SocketException e)
        //    {
        //        if (verbose)
        //        {
        //            //Log error with e
        //        }
        //        return false;
        //    }
        //}

        private void Send(KQMLPerformative msg)
        {
            throw new NotImplementedException();
        }

        private void Register()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                KQMLPerformative perf = new KQMLPerformative("register");
                perf.Set("name", Name);
                if (string.IsNullOrEmpty(GroupName))
                {
                    try
                    {
                        if (GroupName.StartsWith("("))
                        {
                            perf.Sets("group", GroupName);
                        }
                        else
                            perf.Set("group", GroupName);
                    }
                    catch (IOException)
                    {
                        //log error
                    }
                }
                Send(perf);
            }
        }

        public void Ready()
        {
            KQMLPerformative msg = new KQMLPerformative("tell");
            KQMLList content = new KQMLList(new List<object> { "module-status", "ready" });
            msg.Set("content", content);
            Send(msg);
        }

        public void Exit(int n)
        {
            if (IsApplication) { }
            //UNDONE: sys?
        }


        public void receive_eof()
        {
            Exit(0);
        }

        public void HandleException(IOException e)
        {
            throw new NotFiniteNumberException();
        }

        public void ReceiveMessageMissingVerb(KQMLPerformative msg)
        {
            { ErrorReply(msg, "missing verb in performative"); }
        }

        public void ErrorReply(KQMLPerformative msg, string comment)
        {
            KQMLPerformative replyMsg = new KQMLPerformative("error");
            replyMsg.Sets("comment", comment);
            Reply(msg, replyMsg);
        }

        public void Reply(KQMLPerformative msg, KQMLPerformative replyMsg)
        {
            KQMLObject sender = msg.Get("sender");
            if (!sender.Equals(null))
            {
                replyMsg.Set("Receiver", sender);
            }
            KQMLObject replyWith = msg.Get("reply-with");
            if (!replyWith.Equals(null))
            {
                replyMsg.Set("in-reply-to", replyWith);
            }
            Send(replyMsg);
        }

        public void receive_message_missing_content(KQMLPerformative msg)
        {
             ErrorReply(msg, "missing content in performative"); 
        }

        public void receive_ask_if(KQMLPerformative msg, string content)
        {
            ErrorReply(msg, "unexpected performative: ask-if"); 
        }

        public void receive_ask_all(KQMLPerformative msg, string content)
        {
            ErrorReply(msg, "unexpected performative: ask-all"); 
        }

        public void receive_ask_one(KQMLPerformative msg, string content)
        {
             ErrorReply(msg, "unexpected performative: ask-one"); 
        }

        public void receive_stream_all(KQMLPerformative msg, string content)
        {
            ErrorReply(msg, "unexpected performative: stream-all"); 

        }

        //public void receive_tell(KQMLPerformative msg, string content)
        //    // logger.error("unexpected performative: tell");

        public void receive_untell(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: untell"); }

        public void receive_deny(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: deny"); }

        public void receive_insert(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: insert"); }

        public void receive_uninsert(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: uninsert"); }

        public void receive_delete_one(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: delete-one"); }

        public void receive_delete_all(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: delete-all"); }

        public void receive_undelete(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: undelete"); }

        public void receive_achieve(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: achieve"); }

        public void receive_advertise(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: advertise"); }

        public void receive_unadvertise(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: unadvertise"); }

        public void receive_subscribe(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: subscribe"); }

        public void receive_standby(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: standby"); }

        public void receive_register(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: register"); }

        public void receive_forward(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: forward"); }

        public void receive_broadcast(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: broadcast"); }

        public void receive_transport_address(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: transport-address"); }

        public void receive_broker_one(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: broker-one"); }

        public void receive_broker_all(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: broker-all"); }

        public void receive_recommend_one(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: recommend-one"); }

        public void receive_recommend_all(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: recommend-all"); }

        public void receive_recruit_one(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: recruit-one"); }

        public void receive_recruit_all(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: recruit-all"); }

        //public void receive_reply(KQMLPerformative msg, string content)
        // logger.error(msg, "unexpected performative: reply");

        public void receive_request(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: request"); }

        public void receive_eos(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: eos"); }

        //public void eceive_error(KQMLPerformative msg)
        //    // logger.error("Error Received: "%s"" % msg);

        // public void receive_sorry(KQMLPerformative msg)
        //    // logger.error("unexpected performative: sorry");

        // public void receive_ready(KQMLPerformative msg)
        //    // logger.error(msg, "unexpected performative: ready");

        // public void receive_next(KQMLPerformative msg)
        //    { ErrorReply(msg, "unexpected performative: next"); }

        public void receive_rest(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: rest"); }

        public void receive_discard(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: discard"); }

        //public void receive_unregister(KQMLPerformative msg)
        // logger.error(msg, "unexpected performative: unregister");

        public void receive_other_performative(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: " + msg.ToString()); }

    }
}
