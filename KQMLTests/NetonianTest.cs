using System;
using System.Net;
using System.Net.Sockets;
using Companions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KQMLTests
{
    [TestClass]
    public class NetonianTest
    {
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 9000);
        
        [TestMethod]
        public void RegisterTest()
        {
            Netonian net = new Netonian();
        }
    }
}
