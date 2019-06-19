﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace KQML
{
    public class KQMLModule
    {
        public Dictionary<object, object> defaults;

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
        public bool Running;

        //Log log log
        private readonly ILog _log = LogManager.GetLogger(typeof(KQMLModule));

        public KQMLModule()
        {
            Host = "localhost";
            Port = 9000;
            IsApplication = false;
            Testing = false;
            Name = "secret-agent";
            GroupName = "(secrets)";
            //Socket = null;
            Debug = false;

            Dispatcher = null;
            MAX_PORT_TRIES = 100;
            ReplyIdCounter = 1;
            Running = true;

            //TODO: Needs to handle command line argument and change defaults
            // JRW: Worry about that later

            
            Connect(Host, Port);
            Dispatcher = new KQMLDispatcher(this, new KQMLReader(In), "secret-dispatch");
            Register();


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
            Thread t = new Thread(new ThreadStart(Dispatcher.Start));
            t.Start();
            while (Running)
            {
                ConsoleKeyInfo input = Console.ReadKey();
                if (input.KeyChar=='Q')
                {
                    Console.WriteLine("Received shutdown signal...");
                    Dispatcher.Shutdown();
                    Console.WriteLine("Dispatcher shutdown");
                    Running = false;
                }
            }
        }

        public void SubscribeRequest(string reqType)
        {
            KQMLPerformative msg = new KQMLPerformative("subscribe");
            KQMLList content = new KQMLList("request");
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
        //        return false;
        //    }
        //}

        //private bool Connect1(string host, int port, bool verbose)
        //{
        //    try
        //    {
        //        //Crreate Socket
        //        Socket.Connect(host, port);
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

        public void Send(KQMLPerformative msg)
        {
            try
            {
                Console.WriteLine("Attempting to write: " + msg);
                msg.Write(Out);
                Out.Write('\n');
                Out.Flush();
            }
            catch (IOException)
            {
                Console.WriteLine("Write failed");
                Out.Write("\n");
                Out.Flush();
                
            }
        }

        public virtual void Register()
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
                        //TODO: Log errors!
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
            Environment.Exit(n);
        }


        public void ReceiveEof()
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

        public void ReceiveMessageMissingContent(KQMLPerformative msg)
        {
             ErrorReply(msg, "missing content in performative"); 
        }

        public void ReceiveAskIf(KQMLPerformative msg, string content)
        {
            ErrorReply(msg, "unexpected performative: ask-if"); 
        }

        public void ReceiveAskAll(KQMLPerformative msg, string content)
        {
            ErrorReply(msg, "unexpected performative: ask-all"); 
        }

        public void ReceiveAskOne(KQMLPerformative msg, string content)
        {
             ErrorReply(msg, "unexpected performative: ask-one"); 
        }

        public void ReceiveStreamAll(KQMLPerformative msg, string content)
        {
            ErrorReply(msg, "unexpected performative: stream-all"); 

        }

        //public void Receive_tell(KQMLPerformative msg, string content)
        //    // logger.error("unexpected performative: tell");

        public void ReceiveUntell(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: untell"); }

        public void ReceiveDeny(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: deny"); }

        public void ReceiveInsert(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: insert"); }

        public void ReceiveUninsert(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: uninsert"); }

        public void ReceiveDeleteOne(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: delete-one"); }

        public void ReceiveDeleteAll(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: delete-all"); }

        public void ReceiveUndelete(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: undelete"); }

        public void ReceiveAchieve(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: achieve"); }

        public void ReceiveAdvertise(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: advertise"); }

        public void ReceiveUnadvertise(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: unadvertise"); }

        public void ReceiveSubscribe(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: subscribe"); }

        public void ReceiveStandby(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: standby"); }

        public void ReceiveRegister(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: register"); }

        public void ReceiveForward(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: forward"); }

        public void ReceiveBroadcast(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: broadcast"); }

        public void ReceiveTransportAddress(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: transport-address"); }

        public void ReceiveBrokerOne(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: broker-one"); }

        public void ReceiveBrokerAll(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: broker-all"); }

        public void ReceiveRecommendOne(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: recommend-one"); }

        public void ReceiveRecommendAll(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: recommend-all"); }

        public void ReceiveRecruitOne(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: recruit-one"); }

        public void ReceiveRecruitAll(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: recruit-all"); }

        //public void Receive_reply(KQMLPerformative msg, string content)
        // logger.error(msg, "unexpected performative: reply");

        public void ReceiveRequest(KQMLPerformative msg, string content)
        { ErrorReply(msg, "unexpected performative: request"); }

        public void ReceiveEos(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: eos"); }

        //public void eceive_error(KQMLPerformative msg)
        //    // logger.error("Error Received: "%s"" % msg);

        // public void Receive_sorry(KQMLPerformative msg)
        //    // logger.error("unexpected performative: sorry");

        // public void Receive_ready(KQMLPerformative msg)
        //    // logger.error(msg, "unexpected performative: ready");

        // public void Receive_next(KQMLPerformative msg)
        //    { ErrorReply(msg, "unexpected performative: next"); }

        public void ReceiveRest(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: rest"); }

        public void ReceiveDiscard(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: discard"); }

        //public void Receive_unregister(KQMLPerformative msg)
        // logger.error(msg, "unexpected performative: unregister");

        public void ReceiveOtherPerformative(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: " + msg.ToString()); }


        static void Main(string[] args)
        {
            KQMLModule module = new KQMLModule();
            module.Ready();
            module.SubscribeRequest("chicken");
            module.SubscribeTell("egg");

            module.Start();
        }

    }
}