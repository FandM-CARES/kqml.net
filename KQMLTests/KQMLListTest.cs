using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KQML;
using System.Collections.Generic;

namespace KQMLTests
{
    [TestClass]
    public class KQMLListTest
    {
        /* def test_init():
    kl = KQMLList()
    assert(kl.data == [])
    kl = KQMLList('head')
    assert(kl.data == ['head'])
    assert(type(kl.data[0] == KQMLToken))
    kl = KQMLList(['a', 'b'])
    assert(kl.data == ['a', 'b'])
         */
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

        /*
         * def test_gets():
           kl = KQMLList.from_string(b'(:hello "")')
           hello = kl.gets('hello')
           assert(hello == '') */
        
        //TODO: FromString unimplmented. Test skipped.
    }
}
