using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KQML;
using System.Collections.Generic;
using KQML.KQMLExceptions;

namespace KQMLTests
{
    [TestClass]
    public class KQMLPerformativeTest
    {
        [TestMethod]
        public void ConstructorTest()
        {

            KQMLPerformative kp = new KQMLPerformative("tell");
            KQMLToken token = (KQMLToken)kp.Data[0];
            Assert.AreEqual(token.Data, "tell");
            /* kp = KQMLPerformative(KQMLList(['a', ':b', 'c']))
               assert(kp.data[0] == 'a')
               assert(kp.data[1] == ':b')   
       */
            List<object> strings = new List<object> { "a", ":b", "c" };
            KQMLList klist = new KQMLList(strings);
            kp = KQMLPerformative.ListToPerformative(klist);

        }
        /*  kp = KQMLPerformative('tell')
    assert(kp.head() == 'tell')
    kp = KQMLPerformative(KQMLList(['tell', ':content', KQMLList(['success'])]))
    assert(kp.head() == 'tell')
    */
        [TestMethod]
        public void HeadTest()
        {
            KQMLPerformative kp = new KQMLPerformative("tell");
            Assert.AreEqual(kp.Head(), "tell");
            List<object> lst = new List<object> { "tell", ":content", new KQMLList(new List<object> { "success" }) };
            Assert.AreEqual(kp.Head(), "tell");

        }

        [TestMethod]
        public void GetTest()
        {
            List<object> lst = new List<object> { "tell", ":content", new KQMLList(new List<object> { "success" }) };
            KQMLPerformative kp = KQMLPerformative.ListToPerformative(new KQMLList(lst));
            KQMLList success = new KQMLList(new List<object> { "success" });
            Assert.IsTrue(kp.Get("content") is KQMLList);
        }

        [ExpectedException(typeof(KQMLBadPerformativeException))]
        [TestMethod]
        public void BadPerformativeTest()
        {
            List<object> lst = new List<object> { "achieve", "chicken" };
            KQMLPerformative kp = KQMLPerformative.ListToPerformative(new KQMLList(lst));
        
        }

        [TestMethod]
        public void SetTest()
        {
            List<object> lst = new List<object> { "tell", ":content", new KQMLList(new List<object> { "success" }) };
            KQMLPerformative kp = KQMLPerformative.ListToPerformative(new KQMLList(lst));
            KQMLToken newVal = new KQMLToken("value");
            kp.Set(":content", newVal);
            Assert.AreEqual(newVal, kp.Data[2]);
        }
    }
}
