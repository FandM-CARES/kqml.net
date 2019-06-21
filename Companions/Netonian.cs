using KQML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Companions
{
    public delegate List<object> AskDelegate(params object[] arguments);
    public class Netonian : KQMLModule
    {
        public DateTime StartTime;
        public int LocalPort;
        public Dictionary<string, AskDelegate> Asks;

        public Netonian()
        {
            LocalPort = 8950;
            StartTime = DateTime.Now;
            Asks = new Dictionary<string, AskDelegate>();
            Listen();
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
        public void AddAsk(string name, AskDelegate function)
        {
            Asks.Add(name, function);
        }

        public void Listen()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), LocalPort);
            server.Start();

            while (Running)
            {
                TcpClient client = server.AcceptTcpClient();

                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                KQMLReader reader = new KQMLReader(sr);
                Dispatcher.Reader = reader;
            }
        }

        public override void ReceiveAskOne(KQMLPerformative msg, KQMLObject content)
        {
            if (!(content is KQMLList contentList))
                throw new ArgumentException("content not a KQMLList");
            string pred = contentList.Head();
            // find all bounded arguments
            List<KQMLObject> bounded = new List<KQMLObject>();
            foreach (var element in contentList.Data)
            {
                if (element is KQMLString elementString)
                {
                    if (elementString[0] != '?')
                        bounded.Append(elementString);
                }
                else if (element is KQMLToken elementToken)
                {
                    if (elementToken[0] != '?')
                        bounded.Append(elementToken);
                }

            }
            // query with those arguments
            AskDelegate del = Asks[pred];

            // using KQMLObject because I really have no idea what types we are getting here lol
            var results = del(bounded);
            KQMLObject respType = msg.Get("response");
            RespondToQuery(msg, contentList, results, respType);

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
            if (respType == null || respType.Equals(":pattern"))
                RespondWithPattern(msg, content, results);
            else
            {
                RespondWithBindings(msg, content, results);
            }
        }

        public void RespondWithPattern(KQMLPerformative msg, KQMLList content, object results)
        {
            //KQMLList replyContent = new KQMLList(content.Head());
            //List<object> resultsList = (results is List<object> list) ? 
            //    list : new List<object>(){results};
            //int resultIndex = 0;
            //int resultLength = resultsList.Count - 1;

            //// pythonian: len(content.data[1:]
            //int argLength = content.Count - 1;
            //for (int i = 0; i <= argLength; i++)
            //{
            //    // if(content.Data[0] is KQMLToken token || content.Data[0] is KQMLString )
            //    if(i == argLength && resultIndex < resultLength)
            //        replyContent.Append(Listify(resultsList.Skip(resultIndex - 1)));
            //}

            throw new NotImplementedException();
        }

        public object Listify(object results)
        {
            throw new NotImplementedException();
        }

        public void RespondWithBindings(KQMLPerformative msg, KQMLList content, object results)
        {
            throw new NotImplementedException();
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

        static void Main(string[] args)
        {
            Netonian net = new Netonian();
            net.Start();
        }

    }


}
