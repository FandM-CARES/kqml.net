using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Companions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KQMLTests
{
    [TestClass]
    public class NetonianTest
    {
        
        
        [TestMethod]
        public void RegisterTest()
        {
            Thread t = new Thread(TestServer.Execute);
            Netonian net = new Netonian();
            
            net.Register();

        }
    }
}
