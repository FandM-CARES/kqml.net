using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KQML;
using System.Collections.Generic;
using log4net.Config;
using log4net;

namespace KQMLTests
{
    [TestClass]
    public class KQMLTokenTest
    {

        [AssemblyInitialize]
        public static void Configure(TestContext tc)
        {
            XmlConfigurator.Configure(new System.IO.FileInfo("logging.xml"));

        }

        [TestMethod]
        public void ConstrutorTest()
        {
            KQMLToken token = new KQMLToken("token");
            Assert.AreEqual(token.StringValue(), "token");
            token = new KQMLToken();
            Assert.AreEqual(token.StringValue(), "");
        }

        [TestMethod]
        public void LengthTest()
        {
            KQMLToken token = new KQMLToken("token");
            Assert.AreEqual(token.Length, 5);
        }

        [TestMethod]
        public void PackageParserTest()
        {
            KQMLToken token = new KQMLToken("ONT::TOKEN");
            List<string> parsed = token.ParsePackage();
            Assert.AreEqual("ONT", parsed[0]);
            Assert.AreEqual("TOKEN", parsed[1]);
            Assert.IsTrue(token.HasPackage());
            token = new KQMLToken("ONT::|TOKEN|");
            parsed = token.ParsePackage();
            Assert.AreEqual("ONT", parsed[0]);
            Assert.AreEqual("|TOKEN|", parsed[1]);
            Assert.IsTrue(token.HasPackage());
            token = new KQMLToken("ONT::|TOK::EN|");
            parsed = token.ParsePackage();
            Assert.AreEqual("ONT", parsed[0]);
            Assert.AreEqual("|TOK::EN|", parsed[1]);
            Assert.IsTrue(token.HasPackage());
            token = new KQMLToken(":keyword");
            parsed = token.ParsePackage();
            Assert.IsNull(parsed[0]);
            Assert.AreEqual(":keyword", parsed[1]);
            Assert.IsFalse(token.HasPackage());
        }

        [TestMethod]
        public void IsKeywordTest()
        {
            KQMLToken token = new KQMLToken(":keyword");
            Assert.IsTrue(token.IsKeyword());
            token = new KQMLToken("keyword");
            Assert.IsFalse(token.IsKeyword());
        }

        [TestMethod]
        public void EqualsTest()
        {
            Assert.IsTrue(new KQMLToken("token").Equals("token"));
            Assert.IsTrue(new KQMLToken("token").Equals(new KQMLToken("token")));
            Assert.IsTrue(new KQMLToken("token").EqualsIgnoreCase("TOKEN"));
            Assert.IsTrue(new KQMLToken("token").EqualsIgnoreCase(new KQMLToken("TOKEN")));
            Assert.IsFalse(new KQMLToken("token").Equals("token1"));
            Assert.IsFalse(new KQMLToken("token").EqualsIgnoreCase("TOKENS"));
        }
    }
}
