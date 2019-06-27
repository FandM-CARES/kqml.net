using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Companions.Test
{
    class TestAgent : Netonian
    {
        new readonly string Name = "TestAgent";
        private static readonly new ILog Log = LogManager.GetLogger(typeof(TestAgent));
        public TestAgent()
        {
            MethodInfo method = typeof(TestAgent).GetMethod("TestAskReturnList");
            AddAsk("TestAskReturnList");

        }

        public List<string> TestAskReturnList(string input)
        {
            Log.Debug("Testing ask with input" + input.ToString());
            return new List<string> { input };
        }
        static void Main(string[] args)
        {
            _ = XmlConfigurator.Configure(new FileInfo("logging.xml"));
            TestAgent agent = new TestAgent();
            agent.Start();

        }
    }
}
