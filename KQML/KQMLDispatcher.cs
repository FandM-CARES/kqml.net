using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

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

        public void Start()
        {
            try
            {
                for (int i = 0; i < 100000; i++)
                {
                    KQMLPerformative msg = (KQMLPerformative)Reader.ReadPerformative();
                    DispatchMessage(msg);
                    // FIXME: not handling KQMLException
                }
            }
            catch (EndOfStreamException e)
            {
                //TODO: Log
                Receiver.ReceiveEof();
            }
            catch (IOException e)
            {
                Receiver.HandleException(e);
            }
            catch (ArgumentException e)
            {
                //TODO: Log some more
            }
            catch (Exception e)
            {
                Console.Write("I dont't know how to handle keyboard interrupts");

            }
        }

        public void Warn(string msg)
        {
            throw new NotImplementedException();
            // TODO: log
        }

        // Can't shutdown because can't close Reader
        public void Shutdown()
        {
            throw new NotImplementedException();

        }
        private void DispatchMessage(KQMLPerformative msg)
        {
            string verb = msg.Head();
            string replyId, value;  // type unclear  
            if (string.IsNullOrEmpty(verb))
            {
                Receiver.ReceiveMessageMissingVerb(msg);
                return;
            }
            KQMLObject replyIdObj = msg.Get("in-reply-to");
            if (replyIdObj != null)
            {
                replyId = replyIdObj.ToString().ToUpper();
                try
                {

                    value = ReplyContinuations[replyId];
                    value.receive(); // FIXME: what are you receiving??
                    ReplyContinuations.Remove(replyId);
                }
                catch (KeyNotFoundException)
                {
                    // TODO: log

                }
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
            string methodName = "Receive" +
                string.Join("", vl.Split('_').Select((string str) => str.First().ToString().ToUpper() + str.Substring(1)));

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
                        Type thing = Receiver.GetType();
                        MethodInfo method = thing.GetMethod(methodName); //FIXME: there really is a method for Type called GetMethod x_x

                    }

                }
            }
            else if (msgOnlyTypes.Contains(vl))
            {
                foreach (string cmt in msgOnlyTypes)
                {
                    if (vl.Equals(cmt))
                    {
                        //TODO: Invoke method reflexively, but can't right now fsr
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
            ReplyContinuations[replyId.ToUpper()] = cont;
        }
    }
}
