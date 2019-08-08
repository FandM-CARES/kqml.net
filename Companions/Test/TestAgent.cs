using KQML;
using log4net;
using log4net.Config;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Companions.Test
{
    public class TestAgent : Netonian
    {

        public volatile bool TestAchieveCalled;
        public volatile bool TestAskReturnListCalled;
        public volatile bool TestAchieveReturnCalled;

        private static readonly new ILog Log = LogManager.GetLogger(typeof(TestAgent));
        public TestAgent(string name) : base(name)
        {
            TestAchieveCalled = false;
            TestAskReturnListCalled = false;

            AddAsk("TestAskReturnList", "(TestAskReturnList ?_input ?return)", true);
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

        public void TestInsertIntoCompanion(object data)
        {
            Log.Debug($"Testing inserting data into companion with data: {data}");
            InsertData("session-reasoner", data);

        }



        static void Main(string[] args)
        {
            _ = XmlConfigurator.Configure(new FileInfo("logging.xml"));
            TestAgent agent = new TestAgent("NetonianTestAgent");
            Thread.Sleep(20);
            agent.TestInsertIntoCompanion("(Started Netonian)");

            agent.AchieveOnAgent("interaction-manager", "(processKioskUtterance \"Where is Professor Forbus?\")");
            agent.Start();

        }
    }
}
