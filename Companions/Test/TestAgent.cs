using KQML;
using log4net;
using log4net.Config;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


namespace Companions.Test
{
    class TestAgent : Netonian
    {
        
        private static readonly new ILog Log = LogManager.GetLogger(typeof(TestAgent));
        public TestAgent()
        {
            MethodInfo method = typeof(TestAgent).GetMethod("TestAskReturnList");
            AddAsk("TestAskReturnList");
            AddAchieve("TestAchieve");
            AddAchieve("TestAchieveReturn");


        }

        public List<object> TestAskReturnList(KQMLObject input)
        {
            Log.Debug("Testing ask with input "
                      + string.Join(" ", input));
            return new List<object> { "this is so hard" };
        }

        public void TestAchieve(KQMLObject input)
        {
            Log.Debug("Testing Achieve with input " + input);
        }

        public List<object> TestAchieveReturn(KQMLObject input)
        {
            Log.Debug("Testing Achieve with input " + input);
            return new List<object> { "This", "is", "cool" };
        }


        static void Main(string[] args)
        {
            _ = XmlConfigurator.Configure(new FileInfo("logging.xml"));
            TestAgent agent = new TestAgent();
            agent.Start();

        }
    }
}
