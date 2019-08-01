using KQML;
using log4net;
using log4net.Config;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


namespace Companions.Test
{
    public class TestAgent : Netonian
    {

        public volatile bool TestAchieveCalled;
        public volatile bool TestAskReturnListCalled;
        public volatile bool TestAchieveReturnCalled;
        private readonly object truthLock;

        private static readonly new ILog Log = LogManager.GetLogger(typeof(TestAgent));
        public TestAgent()
        {
            TestAchieveCalled = false;
            TestAskReturnListCalled = false;

            AddAsk("TestAskReturnList");
            AddAchieve("TestAchieve");
            AddAchieve("TestAchieveReturn");
        }


        public List<object> TestAskReturnList(KQMLObject input)
        {
            TestAskReturnListCalled = true;
            Log.Debug("Testing ask with input "
                      + string.Join(" ", input));
            return new List<object> { "this is so hard" };
        }

        public void TestAchieve(KQMLObject input)
        {
            TestAchieveCalled = true;
            Log.Debug("Testing Achieve with input " + input);
            Log.Debug("TestAchieveCalled should now be " + TestAchieveCalled);
        }

        public List<object> TestAchieveReturn(KQMLObject input)
        {
            TestAchieveReturnCalled = true;
            Log.Debug("Testing AchieveReturn with input " + input);
            Log.Debug("TestAchieveReturnCalled should now be " + TestAchieveReturnCalled);
            return new List<object> { "This", "is", "cool" };
        }


        static void Main(string[] args)
        {
            _ = XmlConfigurator.Configure(new FileInfo("logging.xml"));
            TestAgent agent = new TestAgent();
            agent.AchieveOnAgent("interaction-manager", "(processKioskUtterance \"Where is Professor Forbus?\")");
            agent.Start();

        }
    }
}
