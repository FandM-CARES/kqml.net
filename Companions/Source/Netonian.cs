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
    //public delegate IEnumerable<object> AskDelegate(params object[] arguments);
    //public delegate IEnumerable<object> AchieveDelegate(object argument);

    public class Netonian : KQMLModule
    {
        public DateTime StartTime;
        public int LocalPort;
        //public Dictionary<string, AskDelegate> Asks;
        //public Dictionary<string, AchieveDelegate> Achieves;
        //public Dictionary<string, MethodInfo> Asks;
        //public Dictionary<string, MethodInfo> Achieves;
        public List<string> Asks;
        public List<string> Achieves;
        public bool Ready;
        public string State;
        public static ILog Log { get; } = LogManager.GetLogger(typeof(Netonian));

        public Netonian() : base()
        {
            LocalPort = 8950;
            StartTime = DateTime.Now;
            Asks = new List<string>();
            Ready = true;
            State = "idle";

            Thread t = new Thread(new ThreadStart(Listen));
            t.Start();
        }
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
        /// Adds name-function mapping to Asks. 
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="function">The function as an AskDelegate</param>
        public void AddAsk(string name)
        {
            Asks.Add(name);
        }

        public void Listen()
        {

            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), LocalPort);
            server.Start();
            Log.Debug("Listening...");

            while (Running)
            {
                //Console.WriteLine("Waiting for a connection...");

                TcpClient client = server.AcceptTcpClient();
                //Console.WriteLine("Connected!");

                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                KQMLReader reader = new KQMLReader(sr);
                //Dispatcher.Reader = reader;

                StreamWriter sw = new StreamWriter(ns);
                Out = sw;

                Dispatcher = new KQMLDispatcher(this, reader, Name);
                Thread t = new Thread(Dispatcher.Start);
                t.Start();

            }

        }

        public override void ReceiveAskOne(KQMLPerformative msg, KQMLObject content)
        {
            Log.Debug($"ReceiveAskOne invoked with {msg} and content {content}");
            if (!(content is KQMLList contentList))
                throw new ArgumentException("content not a KQMLList");
            string pred = contentList.Head() ?? throw new ArgumentNullException("content is null");
            // find all bounded arguments
            List<KQMLObject> bounded = new List<KQMLObject>();
            foreach (var element in contentList.Data)
            {
                if (element is KQMLString elementString)
                {
                    if (elementString[0] != '?')
                        bounded.Add(elementString);
                }
                else if (element is KQMLToken elementToken)
                {
                    if (elementToken[0] != '?')
                        bounded.Add(elementToken);
                }

            }
            // query with those arguments
            MethodInfo del = GetType().GetMethod(pred);

            // type unclear
            if (del != null)
            {
                var results = del.Invoke(this, bounded.ToArray());
                Log.Debug($"{del} invoked with arguments {bounded}. Results were {results}");
                KQMLObject respType = msg.Get("response");
                RespondToQuery(msg, contentList, results, respType);

            }
            else
                Log.Error("no method named " + pred);


        }

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

            ErrorReply(msg, $"Invalid content type: {content}");
        }

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
                        //FIXME: type unclear
                        var results = del.Invoke(this, args.ToArray());
                        Log.Debug("Return of achieve: " + results);

                        KQMLPerformative reply = new KQMLPerformative("tell");
                        reply.Set("sender", Name);
                        var resultsList = Listify(results);
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
            ErrorReply(msg, $"Invalid action type: {action}");
        }

        public override void ReceiveOtherPerformative(KQMLPerformative msg)
        {
            if (msg.Head() == "ping")
                ReceivePing(msg);
            else
            {
                ErrorReply(msg, $"unexpected performative: {msg}");

            }
        }

        public void RespondToQuery(KQMLPerformative msg, KQMLList content, object results, KQMLObject respType)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (respType is KQMLString respTypeString)
            {
                if (respTypeString == null || respTypeString.Equals(":pattern"))
                    RespondWithPattern(msg, content, results);
                else
                {
                    RespondWithBindings(msg, content, results);
                }
            }

        }

        public void RespondWithPattern(KQMLPerformative msg, KQMLList content, object results)
        {
            KQMLList replyContent = new KQMLList(content.Head());
            List<object> resultsList = (results is List<object> list) ?
                list : new List<object>() { results };
            int resultIndex = 0;
            int resultLength = resultsList.Count - 1;

            // pythonian: len(content.data[1:])
            int argLength = content.Count - 1;
            for (int i = 0; i <= argLength; i++)
            {

                if (content.Data[0] is KQMLString indexable)
                {
                    if (indexable[0] == '?')
                    {
                        if (i == argLength && resultIndex < resultLength)
                            replyContent.Append(Listify(resultsList.Skip(resultIndex - 1)));
                        else
                        {
                            replyContent.Append(Listify(resultsList[resultIndex]));
                            resultIndex += 1;
                        }
                    }
                    else
                        replyContent.Append(indexable);
                }

            }
            KQMLPerformative replyMsg = new KQMLPerformative("tell");
            replyMsg.Set("sender", Name);
            replyMsg.Set("content", replyContent);
            Reply(msg, replyMsg);


        }
        public KQMLObject Listify(KeyValuePair<object, object> target)
        {
            var key = target.Key;
            var value = target.Value;

            string resultKey = ":" + key.ToString();
            var resultValue = Listify(value);

            return KQMLList.FromString($"({resultKey} {resultValue})");
        }

        public KQMLObject Listify(object target)
        {
            if (target is string targetString)
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
            else
                return new KQMLToken(target.ToString());
        }

        public KQMLObject Listify(IEnumerable<object> target)
        {
            var targetList = target.Select(Listify).ToList();
            return new KQMLList(Flatten(targetList));
        }

        public List<KQMLObject> Flatten(List<KQMLObject> target)
        {
            List<KQMLObject> flatList = new List<KQMLObject>();
            foreach (var entry in target)
            {
                if (entry is KQMLList assocList && assocList.Count == 2)
                {
                    flatList.Append(assocList[0]);
                    flatList.Append(assocList[1]);
                }
                else
                {
                    flatList.Append(entry);
                }
            }
            return flatList;
        }


        public void RespondWithBindings(KQMLPerformative msg, KQMLList content, object results)
        {
            throw new NotImplementedException();
        }

        public override void ReceiveEof()
        {

        }



        public override void ReceiveTell(KQMLPerformative msg, KQMLObject content)
        {
            KQMLPerformative replyMsg = new KQMLPerformative("tell");
            replyMsg.Set("sender", Name);
            replyMsg.Set("content", ":OK");
            Reply(msg, replyMsg);
        }

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
