using KQML;
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
        
        private static readonly new ILog Log = LogManager.GetLogger(typeof(TestAgent));
        public TestAgent()
        {
            MethodInfo method = typeof(TestAgent).GetMethod("TestAskReturnList");
            AddAsk("TestAskReturnList");

        }

        public List<object> TestAskReturnList(List<KQMLObject> input)
        {
            Log.Debug("Testing ask with input"
                      + String.Join(" ", input));
            return new List<object> { "this is so hard" };
        }
        static void Main(string[] args)
        {
            _ = XmlConfigurator.Configure(new FileInfo("logging.xml"));
            TestAgent agent = new TestAgent();
            agent.Start();

        }
    }
}
