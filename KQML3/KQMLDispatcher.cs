using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using log4net;
using KQML.KQMLExceptions;

namespace KQML
{
    public class KQMLDispatcher
    {
        public KQMLModule Receiver;
        public KQMLReader Reader;
        public string AgentName;
        public Dictionary<string, string> ReplyContinuations;   // use unclear 
        private int Counter;
        public string Name;
        public bool ShutdownIntiated;
        private static readonly ILog _log = LogManager.GetLogger(typeof(KQMLDispatcher));

        /// <summary>
        /// Initializes a new instance of the <see cref="KQMLDispatcher"/> class with specified receiver, input and name
        /// </summary>
        /// <param name="rec">The agent to handle message dispatch</param>
        /// <param name="inp">Reader from which the <see cref="KQMLDispatcher"/> reads</param>
        /// <param name="agentName">Name of the agent</param>
        public KQMLDispatcher(KQMLModule rec, KQMLReader inp, string agentName)
        {
            
            Receiver = rec;
            Reader = inp;
            ReplyContinuations = new Dictionary<string, string>();
            Counter = 0;
            Name = $"KQML-Disptacher-{Counter}";
            AgentName = agentName;
            ++Counter;
            ShutdownIntiated = false;
        }

        /// <summary>
        /// Start reading and dispatching messages from Reader.  
        /// </summary>
        public void Start()
        {
            try
            {
                while (!ShutdownIntiated)
                {
                    KQMLPerformative msg = (KQMLPerformative)Reader.ReadPerformative();
                    DispatchMessage(msg);
                }
            }
            catch (EndOfStreamException)
            {
                _log.Info("Received end-of-stream error");
                Receiver.ReceiveEof();
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
            catch (KQMLExpectedListException)
            {
                _log.Error("ReadPerformative failed. Expected a list");
            }
            catch (Exception e)
            {
                _log.Error("Start: unknown error " + e);
            }

            // Keyboard interrupt handled in module
        }

        /// <summary>
        /// Logs warning with msg
        /// </summary>
        /// <param name="msg"> Message associated with warning</param>
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
        /// <summary>
        /// Identify verb in msg and calls corresponding handler function. Replies with error if msg has no verb or verb has no corresponding action.
        /// </summary>
        /// <param name="msg">
        /// Message to be dispatched. Should contain verb.
        /// </param>
        private void DispatchMessage(KQMLPerformative msg)
        {
            _log.Debug("Dispatching \"" + msg + "\"");
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
                
            }
            string vl = verb.ToLower();
            KQMLObject content = msg.Get("content");

            List<string> contentMsgTypes = new List<string>{"ask-if", "ask-all", "ask-one", "stream-all",
                             "tell", "untell", "deny", "insert", "uninsert",
                             "delete-one", "delete-all", "undelete", "achieve",
                             "unachieve", "advertise", "subscribe", "standby",
                             "register", "forward", "broadcast",
                             "transport-address", "broker-one", "broker-all",
                             "recommend-one", "recommend-all", "recruit-one",
                             "recruit-all", "reply", "request" };
            List<string> msgOnlyTypes = new List<string> { "eos", "error", "sorry", "ready", "next", "next", "rest", "discard", "unregister" };
            char[] delimiters = { '-', '_' };
            string methodName = "Receive" +
                string.Join("", vl.Split(delimiters).Select((string str) => str.First().ToString().ToUpper() + str.Substring(1)));
            

            if (contentMsgTypes.Contains(vl))
            {
                if (content == null)
                {
                    Receiver.ReceiveMessageMissingContent(msg);
                    return;
                }

                foreach (string cmt in contentMsgTypes)
                {
                    if (vl.Equals(cmt))
                    {
                        Type type = Receiver.GetType();
                        MethodInfo method = type.GetMethod(methodName);
                        if (method != null) method.Invoke(Receiver, new object[] { msg, content });
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
                Receiver.ReceiveOtherPerformative(msg);
            }

            return;
        }
        public void AddReplyContinuation(string replyId, string cont)
        {

            _log.Debug($"Adding replyId {replyId} with content \"{ cont} \"");
            // ReplyContinuations[replyId.ToUpper()] = cont;
        }
    }
}
