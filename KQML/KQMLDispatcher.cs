using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using log4net;

namespace KQML
{
    class KQMLDispatcher
    {
        public KQMLModule Receiver;
        public KQMLReader Reader;
        public string AgentName;
        public Dictionary<string, string> ReplyContinuations;   //type unclear
        private int Counter;
        public string Name;
        public bool ShutdownIntiated;
        private readonly static ILog _log = LogManager.GetLogger(typeof(KQMLDispatcher));

        public KQMLDispatcher(KQMLModule rec, KQMLReader inp, string agentName)
        {
            _log.Debug("-----KQMLDispatcher ctor invoked-----");
            Receiver = rec;
            Reader = inp;
            ReplyContinuations = new Dictionary<string, string>();
            Counter = 0;
            Name = $"KQML-Disptacher-{Counter}";
            AgentName = agentName;
            ++Counter;
            ShutdownIntiated = false;
        }

        public void Start()
        {
            try
            {
                while (!ShutdownIntiated)
                {
                    KQMLPerformative msg = (KQMLPerformative)Reader.ReadPerformative();
                    DispatchMessage(msg);
                    // FIXME: not handling KQMLException
                }
            }
            catch (EndOfStreamException)
            {
                _log.Error("Received end-of-stream error");
                Receiver.receive_eof();
            }
            catch (IOException e)
            {
                _log.Error("Received I/O exception");
                Receiver.HandleException(e);
            }
            catch (ArgumentException)
            {
                _log.Error("Invalid argument in Start");
            }
            catch (Exception)
            {
                Console.Write("I dont't know how to handle keyboard interrupts");   //TODO: keyboard interrupt?

            }
        }

        public void Warn(string msg)
        {
            _log.Warn(msg);
        }

        public void Shutdown()
        {
            _log.Debug($"-----KQMLDispatcher {Counter} shutting down-----");
            ShutdownIntiated = true;
            Reader.Close();
        }
        private void DispatchMessage(KQMLPerformative msg)
        {
            _log.Debug("Dispatching message with content \"" + msg.ToString() + "\"");
            string verb = msg.Head();
            string replyId;  // type unclear  
            if (string.IsNullOrEmpty(verb))
            {
                Receiver.ReceiveMessageMissingVerb(msg);
                return;
            }
            KQMLObject replyIdObj = msg.Get("in-reply-to");
            if (replyIdObj != null)
            {
                replyId = replyIdObj.ToString().ToUpper();
                //try
                //{

                //    value = ReplyContinuations[replyId];
                //    value.receive(); // FIXME: what are you receiving??
                //    ReplyContinuations.Remove(replyId);
                //}
                //catch (KeyNotFoundException)
                //{

                //}
            }
            string vl = verb.ToLower();
            KQMLPerformative content = (KQMLPerformative)msg.Get("content");

            List<string> contentMsgTypes = new List<string>{"ask-if", "ask-all", "ask-one", "stream-all",
                             "tell", "untell", "deny", "insert", "uninsert",
                             "delete-one", "delete-all", "undelete", "achieve",
                             "unachieve", "advertise", "subscribe", "standby",
                             "register", "forward", "broadcast",
                             "transport-address", "broker-one", "broker-all",
                             "recommend-one", "recommend-all", "recruit-one",
                             "recruit-all", "reply", "request" };
            List<string> msgOnlyTypes = new List<string> { "eos", "error", "sorry", "ready", "next", "next", "rest", "discard", "unregister" };
            //string methodName = "Receive" +
            //    string.Join("", vl.Split('_').Select((string str) => str.First().ToString().ToUpper() + str.Substring(1)));
            string methodName = "receive_" + vl.Replace('-', '_');

            if (contentMsgTypes.Contains(vl))
            {
                if (content == null)
                {
                    Receiver.receive_message_missing_content(msg);
                    return;
                }

                foreach (string cmt in contentMsgTypes)
                {
                    if (vl.Equals(cmt))
                    {
                        Type type = Receiver.GetType();
                        MethodInfo method = type.GetMethod(methodName);
                        method.Invoke(Receiver, new object[] { msg, content });

                    }

                }
            }
            else if (msgOnlyTypes.Contains(vl))
            {
                foreach (string cmt in msgOnlyTypes)
                {
                    if (vl.Equals(cmt))
                    {
                        Type type = Receiver.GetType();
                        MethodInfo method = type.GetMethod(methodName);
                        method.Invoke(Receiver, new object[] { msg });
                    }
                }
            }
            else
            {
                Receiver.receive_other_performative(msg);
            }

            return;
        }
        public void AddReplyContinuation(string replyId, string cont)
        {

            _log.Debug($"Adding replyId {replyId} with content \"{ cont} \"");
            ReplyContinuations[replyId.ToUpper()] = cont;
        }
    }
}
