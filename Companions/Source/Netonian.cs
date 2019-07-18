using KQML;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace Companions
{

    /// <summary>
    /// Represents an agent interfacing with Companions
    /// </summary>
    public class Netonian : KQMLModule
    {
        public DateTime StartTime;
        public int LocalPort;
        public List<string> Asks;
        public List<string> Achieves;
        public bool Ready;
        public string State;
        public static ILog Log { get; } = LogManager.GetLogger(typeof(Netonian));

        /// <summary>
        /// Initializes a new instance <see cref="Netonian"/>. Spawns a new thread and waits for connection
        /// </summary>
        public Netonian() : base()
        {
            LocalPort = 8950;
            StartTime = DateTime.Now;
            Asks = new List<string>();
            Achieves = new List<string>();
            Ready = true;
            State = "idle";

            Thread t = new Thread(new ThreadStart(Listen));
            t.Start();
        }

        /// <summary>
        /// Sends a register message to Companions with Name. 
        /// </summary>
        /// <remarks>Agent should appear in session manager as a result</remarks>
        public override void Register()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                KQMLPerformative perf = new KQMLPerformative("register");
                perf.Set("sender", Name);
                perf.Set("receiver", "facilitator");
                string ip = "127.0.0.1";
                KQMLList content = new KQMLList(new List<object> { $"\"socket://{ip}:8950\"", "nil", "nil", 8950 });
                perf.Set("content", content);
                Send(perf);
            }
        }

        /// <summary>
        /// Add name to the list of possible Asks
        /// </summary>
        /// <param name="name">name of function to be used with ask-one</param>
        public void AddAsk(string name)
        {
            Asks.Add(name);
        }

        /// <summary>
        /// Add name to the list of possible Achieves
        /// </summary>
        /// <param name="name">name of function to be used with achieve</param>
        public void AddAchieve(string name)
        {
            Achieves.Add(name);
        }

        /// <summary>
        /// Starts a server and wait for connection. Creates a new thread with a dispatcher once connection is made
        /// </summary>
        public void Listen()
        {

            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), LocalPort);
            server.Start();
            Log.Debug("Listening...");

            while (Running)
            {

                TcpClient client = server.AcceptTcpClient();

                // Wrap network stream in KQMLReader
                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                KQMLReader reader = new KQMLReader(sr);

                // Create an output stream
                StreamWriter sw = new StreamWriter(ns);
                Out = sw;

                Dispatcher = new KQMLDispatcher(this, reader, Name);
                Thread t = new Thread(Dispatcher.Start);
                t.Start();

            }

        }
        /// <summary>
        /// Calls a function in Ask and responds to ask-one message with its return value
        /// </summary>
        /// <param name="msg">ask-one message</param>
        /// <param name="content">content of message</param>
        public override void ReceiveAskOne(KQMLPerformative msg, KQMLObject content)
        {
            Log.Debug($"ReceiveAskOne called with {msg} and content {content}");
            if (!(content is KQMLList contentList))
                throw new ArgumentException("content not a KQMLList");
            string pred = contentList.Head() ?? throw new ArgumentNullException("content is null");

            // find all bounded arguments
            List<object> bounded = new List<object>();
            foreach (var element in contentList.Data.Skip(1))
            {
                if (element.ToString()[0] != '?')
                    bounded.Add(element);

            }
            // Query for method with called pred
            MethodInfo del = GetType().GetMethod(pred);

            // if method found, invoke with bounded arguments
            if (del != null)
            {
                var results = del.Invoke(this, bounded.ToArray());
                Log.Debug(message: $"{del} invoked with arguments {bounded}. Results were {results}");
                KQMLObject respType = msg.Get("response");
                RespondToQuery(msg, contentList, (List<object>)results, respType);

            }
            else
                Log.Error("no method named " + pred);


        }

        /// <summary>
        /// Checks if achieve message contains the correct value for keyword "action". Replies with error if it does not
        /// </summary>
        /// <param name="msg">"achieve" message</param>
        /// <param name="content">content of message</param>
        public override void ReceiveAchieve(KQMLPerformative msg, KQMLObject content)
        {

            if (content is KQMLList contentList)
            {
                if (contentList.Head().Equals("task"))
                {
                    KQMLObject action = contentList.Get("action");
                    if (action != null)
                        HandleAchieveAction(msg, contentList, action);
                    else
                    {

                        ErrorReply(msg, "no action for achieve task provided");
                    }
                }
                else if (contentList.Head().Equals("actionSequence"))
                    ErrorReply(msg, "unexpected achieve command: actionSequence");
                else if (contentList.Head().Equals("eval"))
                    ErrorReply(msg, "unexpected achieve command: eval");
                else
                {
                    ErrorReply(msg, $"unexpected achieve command: {contentList.Head()}");
                }

            }
            else
                ErrorReply(msg, $"Invalid content type: {content}");
        }

        /// <summary>
        /// Calls a function in <see cref="Achieves"/> and respond with its return values
        /// </summary>
        /// <param name="msg">The original achieve message</param>
        /// <param name="contentList">content of message, not being used in method</param>
        /// <param name="action">KQMLList containing the action to achieve, as well as necessary arguments</param>
        public void HandleAchieveAction(KQMLPerformative msg, KQMLList contentList, KQMLObject action)
        {
            if (action is KQMLList actionList)
            {
                if (Achieves.Contains(actionList.Head()))
                {
                    try
                    {
                        List<KQMLObject> args = actionList.Data.Skip(1).ToList();
                        MethodInfo del = this.GetType().GetMethod(actionList.Head());
                        var results = del.Invoke(this, args.ToArray());

                        dynamic resultsList = new KQMLString();
                        if (results != null)
                            resultsList = Listify((dynamic)results);
                        else
                            resultsList = new KQMLString("nil");
                        Log.Debug("Return of achieve: " + resultsList);

                        KQMLPerformative reply = new KQMLPerformative("tell");
                        reply.Set("sender", Name);
                        reply.Set("content", resultsList);
                        Reply(msg, reply);
                    }
                    catch (Exception e)
                    {
                        StackTrace st = new StackTrace(new StackFrame(true));
                        Log.Debug(st.ToString(), e);
                        ErrorReply(msg, $"An error occurred while executing {actionList.Head()}");
                    }
                }
                else
                {
                    ErrorReply(msg, $"unknown action {actionList.Head()}");
                }
            }
            else
                ErrorReply(msg, $"Invalid action type: {action}");
        }

        /// <summary>
        /// Handles non-achieve/ask-one messages
        /// </summary>
        /// <param name="msg">message to be handled</param>
        public override void ReceiveOtherPerformative(KQMLPerformative msg)
        {
            if (msg.Head() == "ping")
                ReceivePing(msg);
            else
            {
                ErrorReply(msg, $"unexpected performative: {msg}");

            }
        }

        /// <summary>
        /// Send a formatted response as a reply
        /// </summary>
        /// <param name="msg">message to be responded</param>
        /// <param name="content">content of original message</param>
        /// <param name="results">results of query, to be incorporated into the response message</param>
        /// <param name="respType">Determines format of response</param>
        public void RespondToQuery(KQMLPerformative msg, KQMLList content, List<object> results, KQMLObject respType)
        {
            if (respType is KQMLString respTypeString)
            {
                if (respTypeString == null || respTypeString.Equals(":pattern"))
                    RespondWithPattern(msg, content, results);
                else
                {
                    RespondWithBindings(msg, content, results);
                }
            }
            else
                RespondWithPattern(msg, content, results);

        }

        /// <summary>
        /// Send a response message formatted as pattern
        /// </summary>
        /// <param name="msg">message to be responded to</param>
        /// <param name="content">content of original message</param>
        /// <param name="results">results of query</param>
        public void RespondWithPattern(KQMLPerformative msg, KQMLList content, List<object> results)
        {
            KQMLList replyContent = new KQMLList(content.Head());
            int resultIndex = 0;

            int argLength = content.Count - 1;
            int i = 0;

            // Populate replyContent with bounded variables and results to unbounded vars 
            foreach (var each in content.Data.Skip(1))
            {
                if (each is KQMLString indexable)
                {
                    if (indexable[0] == '?')
                    {
                        if (i == argLength && resultIndex < results.Count - 1)
                            replyContent.Append(Listify(results.Skip(resultIndex - 1)));
                        else
                        {
                            replyContent.Append(Listify((dynamic)results[resultIndex]));
                            resultIndex += 1;
                        }
                    }
                    else
                        replyContent.Append(indexable);
                }
                ++i;

            }

            KQMLPerformative replyMsg = new KQMLPerformative("tell");
            replyMsg.Set("sender", Name);
            replyMsg.Set("content", replyContent);
            Reply(msg, replyMsg);

        }

        /// <summary>
        /// Converts <paramref name="target"/> into a List
        /// </summary>
        /// <param name="target">KeyValuePair to be converted</param>
        /// <returns>A KQMLList in the form of associative list</returns>
        /// <remarks>Colons are NOT added by default for keywords when handling dictioaries. Add your own colons </remarks>

        public KQMLObject Listify(KeyValuePair<object, object> target)
        {
            var key = target.Key;
            var value = target.Value;

            string resultKey = ":" + key.ToString();
            var resultValue = Listify((dynamic)value);

            return KQMLList.FromString($"({resultKey} . {resultValue})");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns>A KQMLList in the form of (key . value)</returns>
        /// <remarks>This overload is required to convert <c>bindingList</c> in <c>RespondWithBinding</c></remarks>
        public KQMLObject Listify(KeyValuePair<KQMLString, List<object>> target)
        {
            var key = target.Key;
            var value = target.Value;

            string resultKey = key.ToString();
            var resultValue = Listify((dynamic)value);

            return KQMLList.FromString($"({resultKey} . {resultValue})");
        }
        /// <summary>
        /// Convert a string into a KQMLObject
        /// </summary>
        /// <param name="targetString">string to be converted</param>
        /// <returns>A KQMLObject</returns>
        public KQMLObject Listify(string targetString)
        {

            if (targetString.Contains(" "))
            {
                // TODO: Incomplete
                if (targetString[0] == '(' && targetString.Last() == ')')
                {
                    List<string> terms = targetString.Substring(1, targetString.Length - 2).Split(' ').ToList();
                    return new KQMLList(terms.Select(Listify).ToList());
                }
                else
                    return new KQMLString(targetString);

            }
            else
                return new KQMLToken(targetString);
        }

        /// <summary>
        /// Converts an integer to a KQMLObject(KQMLToken)
        /// </summary>
        /// <param name="target">Integer to be converted</param>
        /// <returns>KQMLToken containing the integer</returns>
        public KQMLObject Listify(int target)
        {
            return new KQMLToken(target.ToString());

        }

        /// <summary>
        /// Converts a character into a KQMLObject(KQMLToken)
        /// </summary>
        /// <param name="target">char to be converted</param>
        /// <returns>KQMLToken containing the char</returns>
        public KQMLObject Listify(char target)
        {
            return new KQMLToken(target.ToString());

        }
        /// <summary>
        /// Converts a <see cref="bool"/> into a KQMLObject(KQMLToken)
        /// </summary>
        /// <param name="target">bool to be converted</param>
        /// <returns>KQMLToken containing the bool</returns>
        public KQMLObject Listify(bool target)
        {
            return new KQMLToken(target.ToString());

        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{object}"/> into a KQMLObject(KQMLToken)
        /// </summary>
        /// <param name="target">IEnumerable to be converted</param>
        /// <returns>KQMLList with content of <paramref name="target"/></returns>
        public KQMLObject Listify(IEnumerable<object> target)
        {

            List<KQMLObject> targetList = new List<KQMLObject>();
            //= ((dynamic)target).Select(Listify).ToList();
            foreach (var each in target)
            {
                targetList.Add((KQMLObject)Listify((dynamic)each));
            }
            return new KQMLList(Flatten(targetList));
        }

        /// <summary>
        /// Convert List<KeyValuePair<KQMLString, List<object>>> to a KQMLObject
        /// </summary>
        /// <param name="target">List<KeyValuePair<KQMLString, List<object>>> to be converted</param>
        /// <returns>a KQMLList with content of <paramref name="target"/></returns>
        /// Used to create <c>bindingList</c> for <see cref="RespondWithBindings(KQMLPerformative, KQMLList, List{object})"/>
        public KQMLObject Listify(List<KeyValuePair<KQMLString, List<object>>> target)
        {

            List<KQMLObject> targetList = new List<KQMLObject>();
            //= ((dynamic)target).Select(Listify).ToList();
            foreach (var each in target)
            {
                targetList.Add((KQMLObject)Listify((dynamic)each));
            }
            return new KQMLList(Flatten(targetList));
        }

        /// <summary>
        /// Flatten a nested List
        /// </summary>
        /// <param name="target">a nested list of KQMLObjects"</param>
        /// <returns>an unnested list of KQMLObjects</returns>
        public List<KQMLObject> Flatten(List<KQMLObject> target)
        {
            List<KQMLObject> flatList = new List<KQMLObject>();
            foreach (var entry in target)
            {
                if (entry is KQMLList assocList && assocList.Count == 2)
                {
                    flatList.Add(assocList[0]);
                    flatList.Add(assocList[1]);
                }
                else
                {
                    flatList.Add(entry);
                }
            }
            return flatList;
        }

        /// <summary>
        /// Sends a response message formatted with bindings
        /// </summary>
        /// <param name="msg">The message to be responded to</param>
        /// <param name="content">Content of original message</param>
        /// <param name="results">Results of query made in <paramref name="msg"/></param>
        public void RespondWithBindings(KQMLPerformative msg, KQMLList content, List<object> results)
        {
            int resultIndex = 0;
            int argLength = content.Count - 1;
            List<KeyValuePair<KQMLString, List<object>>> bindingsList = new List<KeyValuePair<KQMLString, List<object>>>();


            int i = 0;
            foreach (var each in content.Data.Skip(1))
            {
                if (each is KQMLString indexable)
                {
                    if (indexable[0] == '?')
                    {
                        if (i == argLength && resultIndex < results.Count - 1)
                        {
                            KeyValuePair<KQMLString, List<object>> pair =
                                new KeyValuePair<KQMLString, List<object>>(indexable, results.Skip(resultIndex - 1).ToList());
                            bindingsList.Add(pair);
                        }
                        else
                        {
                            KeyValuePair<KQMLString, List<object>> pair = new KeyValuePair<KQMLString, List<object>>(indexable, new List<object> { results[resultIndex] });
                            resultIndex += 1;
                            bindingsList.Add(pair);
                        }
                    }
                    ++i;
                }
            }
            KQMLPerformative replyMsg = new KQMLPerformative("tell");
            replyMsg.Set("sender", Name);
            replyMsg.Set("content", Listify((dynamic)bindingsList));
            Reply(msg, replyMsg);
        }

        public override void ReceiveEof()
        {

        }


        /// <summary>
        /// Send a tell message of ok in response to a tell mesage
        /// </summary>
        /// <param name="msg">The tell message to be responded to</param>
        /// <param name="content">Content of tell message. Gets logged</param>
        public override void ReceiveTell(KQMLPerformative msg, KQMLObject content)
        {
            Log.Debug($"Received tell: {content}");
            KQMLPerformative replyMsg = new KQMLPerformative("tell");
            replyMsg.Set("sender", Name);
            replyMsg.Set("content", ":OK");
            Reply(msg, replyMsg);
        }

        /// <summary>
        /// Send a reply to a ping message
        /// </summary>
        /// <param name="msg">The message to be responded to</param>
        public void ReceivePing(KQMLPerformative msg)
        {
            KQMLPerformative reply = new KQMLPerformative("update");
            reply.Set("sender", Name);
            KQMLList replyContent = new KQMLList(new List<object> { ":agent", Name });
            replyContent.Append("uptime");
            replyContent.Append(Uptime());
            replyContent.Append(":status");
            replyContent.Append(":OK");
            replyContent.Append(":state");
            replyContent.Append("idle");
            replyContent.Append(":machine");
            replyContent.Append(Dns.GetHostName());
            reply.Set("content", replyContent);
            Reply(msg, reply);
        }

        /// <summary>
        /// Wrap some data in an achieve message and send
        /// </summary>
        /// <param name="receiver">The intended recipient of message</param>
        /// <param name="data">The content to be incorporated in the achieve message</param>
        public void AchieveOnAgent(string receiver, object data)
        {
            try
            {
                KQMLPerformative msg = new KQMLPerformative("achieve");
                msg.Set("sender", Name);
                msg.Set("receiver", receiver);
                msg.Set("content", Listify((dynamic)data));
                Connect(Host, Port);
                Send(msg);
            }
            catch (ArgumentException)
            {
                Log.Error("Cannot Listify data of this type: " + data);
            }
            catch (Exception)
            {
                Log.Error("AchieveOnAgent failed for unknown reason");
            }
        }


        /// <summary>
        /// Calculates how long the agent has been connected
        /// </summary>
        /// <returns>String representation of agent's uptime</returns>
        private string Uptime()
        {
            DateTime now = DateTime.Now;
            int years = now.Year - StartTime.Year;
            int months, days, hours, seconds, minutes;
            List<int> longMonths = new List<int> { 1, 3, 5, 7, 8, 10, 12 };
            List<int> shortMonths = new List<int> { 4, 6, 9, 11 };
            // months
            if (now.Year == StartTime.Year)
                months = now.Month - StartTime.Month;
            else
            {
                months = 12 - StartTime.Month + now.Month;
            }

            // Days
            if (now.Month == StartTime.Month)
                days = now.Day - StartTime.Day;
            else if (longMonths.Contains(now.Month))
                days = 31 - StartTime.Day + now.Day;
            else if (shortMonths.Contains(now.Month))
                days = 30 - StartTime.Day + now.Day;
            else
                days = 29 - StartTime.Day + now.Day;

            //Hours 
            if (StartTime.Day == now.Day)
                hours = now.Hour - StartTime.Hour;
            else
            {
                hours = 24 - StartTime.Hour + now.Hour;
            }

            // Minutes
            if (StartTime.Hour == now.Hour)
                minutes = now.Minute - StartTime.Minute;
            else
            {
                minutes = 60 - StartTime.Minute + now.Minute;
            }

            //Seconds
            if (StartTime.Minute == now.Minute)
                seconds = now.Second - StartTime.Second;
            else
            {
                seconds = 60 - StartTime.Second + now.Second;
            }
            return $"({years} {months} {days} {hours} {minutes} {seconds})";

        }

        public static int test(string foo)
        {
            Console.WriteLine(foo);
            return 1;
        }

        static void Main(string[] args)
        {
            // Log log log
            _ = XmlConfigurator.Configure(new FileInfo("logging.xml"));

            Netonian net = new Netonian();

            net.Start();
        }

    }


}
