using KQML;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Text;


namespace Companions
{
    public class Netonian : KQMLModule
    {
        public DateTime StartTime;
        public int LocalPort;
        // public string Name;
        public Netonian() : base()
        {
            LocalPort = 8950;
            StartTime = DateTime.Now;

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

        public void Listen()
        {
            //TODO: Concurrent threading




        }

        public void ReceiveTell(KQMLPerformative msg, string content)
        {
            KQMLPerformative replyMsg = new KQMLPerformative("tell");
            replyMsg.Set("sender", Name);
            replyMsg.Set("content", null);
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
            List<int> shortMonths = new List<int> {4, 6, 9, 11};
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
