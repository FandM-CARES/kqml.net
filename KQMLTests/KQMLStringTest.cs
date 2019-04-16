using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KQML;

namespace KQMLTests
{
    [TestClass]
    public class KQMLStringTest
    {
        [TestMethod]
        public void LengthTest()
        {
            string s = "hello\nhe\"ll\"o";
            KQMLString kstr = new KQMLString(s);
            string ss = kstr.ToString();
            Assert.AreEqual(s.Length, ss.Length);
        }

        [TestMethod]
        public void IndexerTest()
        {
            KQMLString kstr = new KQMLString("chicken");
            Assert.AreEqual('h', kstr[1]);
        }

    }
}
