﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
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
        public TcpClient Socket;
        public string Name;
        public bool ScanForPort;
        public bool Debug;

        public StreamReader In;
        public StreamWriter Out;
        public string GroupName;
        public bool Running;

        public static ILog Log { get; } = LogManager.GetLogger(typeof(KQMLModule));

        /// <summary>
        /// Constructor. Should handle kwargs soon.
        /// </summary>
        public KQMLModule(string name)
        {
            Host = "localhost";
            Port = 9000;
            IsApplication = false;
            Testing = false;
            Name = name;
            GroupName = "(secrets)";

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

        /// <summary>
        /// Create a TcpClient at <paramref name="host"/>and <paramref name="port"/> set In and Out using its stream
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        protected void Connect(string host, int port)
        {
            Socket = new TcpClient(host, port);
            NetworkStream ns = Socket.GetStream();
            In = new StreamReader(ns);
            Out = new StreamWriter(ns);
        }
        /// <summary>
        /// Start KQMLModule by starting a the dispatcher
        /// </summary>
        public void Start()
        {
            //Thread t = new Thread(new ThreadStart(Dispatcher.Start));
            //t.Start();
            //while (Running)
            //{
            //    ConsoleKeyInfo input = Console.ReadKey();
            //    if (input.KeyChar=='Q')
            //    {
            //        Console.WriteLine("Received shutdown signal...");
            //        Dispatcher.Shutdown();
            //        Console.WriteLine("Dispatcher shutdown");
            //        Running = false;
            //    }
            //}

            Dispatcher.Start();
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


        /// <summary>
        /// Write a message to Out
        /// </summary>
        /// <param name="msg">Message to be sent</param>
        public void Send(KQMLPerformative msg)
        {
            try
            {
                //Console.WriteLine("Attempting to write: " + msg);
                msg.Write(Out);
                Out.Write('\n');
                Out.Flush();
                Log.Debug("Written message " + msg);
            }
            catch (IOException)
            {
                Console.WriteLine("Write failed");
                Out.Write("\n");
                Out.Flush();
            }
        }

        /// <summary>
        /// Send a register message with name of agent
        /// </summary>
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

        /// <summary>
        /// Sends a tell message that a module is ready
        /// </summary>
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

        /// <summary>
        /// Calls <see cref="Exit(int)"/> upon <see cref="EndOfStreamException"/>
        /// </summary>
        public virtual void ReceiveEof()
        {
            Exit(0);
        }

        /// <summary>
        /// handles exception by logging
        /// </summary>
        /// <param name="e">exception to be handled</param>
        public void HandleException(Exception e)
        {
            Log.Error(e);
        }

        public void ReceiveMessageMissingVerb(KQMLPerformative msg)
        {
            { ErrorReply(msg, "missing verb in performative"); }
        }

        /// <summary>
        /// Send a reply message that an error has occured
        /// </summary>
        /// <param name="msg">The mesage to be responded to</param>
        /// <param name="comment">description of error</param>
        public void ErrorReply(KQMLPerformative msg, string comment)
        {
            KQMLPerformative replyMsg = new KQMLPerformative("error");
            replyMsg.Sets("comment", comment);
            Reply(msg, replyMsg);
        }

        /// <summary>
        /// Format and send a reply message to msg with content replyMsg
        /// </summary>
        /// <param name="msg">message that needs replying to</param>
        /// <param name="replyMsg">content for replying</param>
        public void Reply(KQMLPerformative msg, KQMLPerformative replyMsg)
        {
            KQMLObject sender = msg.Get("sender");
            if (sender != null)
            {
                replyMsg.Set("receiver", sender);
            }
            KQMLObject replyWith = msg.Get("reply-with");
            if (replyWith != null)
            {
                replyMsg.Set("in-reply-to", replyWith);
            }
            Send(replyMsg);
        }

        // The following functions should be overriden. Classes extending KQMLModule should handle performatives accordingly
        public virtual void ReceiveMessageMissingContent(KQMLPerformative msg)
        {
            ErrorReply(msg, "missing content in performative");
        }

        public virtual void ReceiveAskIf(KQMLPerformative msg, KQMLObject content)
        {
            ErrorReply(msg, "unexpected performative: ask-if");
        }

        public virtual void ReceiveAskAll(KQMLPerformative msg, KQMLObject content)
        {
            ErrorReply(msg, "unexpected performative: ask-all");
        }

        public virtual void ReceiveAskOne(KQMLPerformative msg, KQMLObject content)
        {
            ErrorReply(msg, "unexpected performative: ask-one");
        }

        public virtual void ReceiveStreamAll(KQMLPerformative msg, KQMLObject content)
        {
            ErrorReply(msg, "unexpected performative: stream-all");

        }

        public virtual void ReceiveTell(KQMLPerformative msg, KQMLObject content)
        {
            ErrorReply(msg, "unexpected performative: stream-all");
        }

        public virtual void ReceiveUntell(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: untell"); }

        public virtual void ReceiveDeny(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: deny"); }

        public virtual void ReceiveInsert(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: insert"); }

        public virtual void ReceiveUninsert(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: uninsert"); }

        public virtual void ReceiveDeleteOne(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: delete-one"); }

        public virtual void ReceiveDeleteAll(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: delete-all"); }

        public virtual void ReceiveUndelete(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: undelete"); }

        public virtual void ReceiveAchieve(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: achieve"); }

        public virtual void ReceiveAdvertise(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: advertise"); }

        public virtual void ReceiveUnadvertise(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: unadvertise"); }

        public virtual void ReceiveSubscribe(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: subscribe"); }

        public virtual void ReceiveStandby(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: standby"); }

        public virtual void ReceiveRegister(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: register"); }

        public virtual void ReceiveForward(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: forward"); }

        public virtual void ReceiveBroadcast(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: broadcast"); }

        public virtual void ReceiveTransportAddress(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: transport-address"); }

        public virtual void ReceiveBrokerOne(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: broker-one"); }

        public virtual void ReceiveBrokerAll(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: broker-all"); }

        public virtual void ReceiveRecommendOne(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: recommend-one"); }

        public virtual void ReceiveRecommendAll(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: recommend-all"); }

        public virtual void ReceiveRecruitOne(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: recruit-one"); }

        public virtual void ReceiveRecruitAll(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: recruit-all"); }

        public virtual void ReceiveReply(KQMLPerformative msg, string content)
        {
            Log.Error("Unexpected performative: reply");
        }

        public virtual void ReceiveRequest(KQMLPerformative msg, KQMLObject content)
        { ErrorReply(msg, "unexpected performative: request"); }

        public virtual void ReceiveEos(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: eos"); }

        public virtual void ReceiveError(KQMLPerformative msg)
        {
            Log.Error($"Error Received: {msg}");
        }

        public virtual void ReceiveSorry(KQMLPerformative msg)
        {
            Log.Error("unexpected performative: sorry");
        }


        public virtual void ReceiveReady(KQMLPerformative msg)
        {
            Log.Error($"Unexpected performative: ready");
        }

        public virtual void ReceiveNext(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: next"); }

        public virtual void ReceiveRest(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: rest"); }

        public virtual void ReceiveDiscard(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: discard"); }

        public void ReceiveUnregister(KQMLPerformative msg)
        {
            Log.Error("unexpected performative: unregister");
        }

        public virtual void ReceiveOtherPerformative(KQMLPerformative msg)
        { ErrorReply(msg, "unexpected performative: " + msg.ToString()); }


        static void Main(string[] args)
        {
            KQMLModule module = new KQMLModule("secret-agent");
            module.Ready();
            module.SubscribeRequest("chicken");
            module.SubscribeTell("egg");

            module.Start();
        }

    }
}
