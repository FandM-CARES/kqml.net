using System;
using System.CodeDom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KQML;
using System.Collections.Generic;

namespace KQMLTests
{
    [TestClass]
    public class KQMLListTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            KQMLList kl = new KQMLList();
            Assert.IsTrue(kl.Data is List<KQMLObject>);
            kl = new KQMLList("head");
            Assert.AreEqual(new KQMLToken("head"), kl.Data[0]);
            kl = new KQMLList(new List<object> { "a", "b" });
            Assert.IsTrue(kl.Data[0] is KQMLObject);
            Assert.IsTrue(kl.Data[1] is KQMLObject);
            Assert.AreEqual("a", kl.Data[0].ToString());
        }


        //def test_gets():
        // kl = KQMLList.from_string(b'(:hello "")') to 
        // hello = kl.gets('hello')
        // assert(hello == '') 

        [TestMethod]
        public void FromStringBasicTest()
        {
            KQMLList kl = KQMLList.FromString("(:hello \"\")");
            string hello = kl.Gets("hello");
            Assert.AreEqual("", hello);
        }

        [TestMethod]
        public void GetTest()
        {
            KQMLList kl = KQMLList.FromString("(ask-all :content \"geoloc(lax,[Long, Lat])\" :language standard_prolog :ontology gee-model3)");
            KQMLObject content = kl.Get("content");
            KQMLString kqmlString = new KQMLString("geoloc(lax,[Long, Lat])");
            Assert.AreEqual(kqmlString, content);
        }

        [TestMethod]
        public void PushStringTest()
        {
            KQMLList kl = new KQMLList("hello");
            kl.Push("chicken");
            Assert.AreEqual("chicken", kl.Head());
        }

        [TestMethod]
        public void PushKqmlTokenTest()
        {
            KQMLList kl = new KQMLList("hello");
            kl.Push(new KQMLToken("chicken"));
            Assert.AreEqual("chicken", kl.Head());
        }

        [TestMethod]
        public void InsertStringAtTest()
        {
            KQMLList kl = new KQMLList(":chicken");
            kl.InsertAt(1, "egg");
            Assert.AreEqual("egg", kl[1].ToString());
        }

        [TestMethod]
        public void InsertKqmlAtTest()
        {
            KQMLList kl = new KQMLList("chicken");
            KQMLToken token = new KQMLToken("egg");
            kl.InsertAt(1, token);
            Assert.AreEqual(new KQMLToken("egg"), kl[1]);
        }

        [TestMethod]
        public void RemoveAtTest()
        {
            List<object> lst = new List<object> { "chicken", "egg" };
            KQMLList kl = new KQMLList(lst);
            kl.RemoveAt(1);
            Assert.AreEqual(1, kl.Count);
        }

        [ExpectedException(typeof(IndexOutOfRangeException))]
        [TestMethod]
        public void RemoveAtOutOfBoundsTest()
        {
            KQMLList kl = new KQMLList("chicken");
            kl.RemoveAt(1);
        }



    }
}
