using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Companions;
using Companions.Test;
using KQML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KQMLTests
{
    [TestClass]
    public class NetonianTest
    {
        TcpListener server;
        TestServer test;


        [TestMethod]
        public void RegisterTest()
        {
            //server = new TcpListener(IPAddress.Parse("127.0.0.1"), 9000);
            //server.Start();
            test = new TestServer();
            test.StartServer();

            Netonian net = new Netonian();

            net.Register();
            KQMLList list = KQMLList.FromString(test.stuff);
            Assert.AreEqual("secret-agent", list.Gets("sender"));
            Assert.AreEqual("facilitator", list.Gets("receiver"));
            Assert.AreEqual("(\"socket://127.0.0.1:8950\" nil nil 8950)", list.Gets("content"));
            test.Stop();
        }

        [TestMethod]
        public void TestingAchieve()
        {
            test = new TestServer();
            test.StartServer();

            TestAgent agent = new TestAgent();

            test.StartAchieveTestClient();
            Thread.Sleep(1000);
            Console.Write("TestAchieveReturned is " + agent.TestAchieveReturnCalled);
            Assert.IsTrue(agent.TestAchieveCalled);            
        }

        
    }
}
