using Microsoft.VisualStudio.TestTools.UnitTesting;

using Morpheus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSUnitTests.DataStructs
{
    [TestClass]
    [TestCategory( "Data Structures" )]
    public class EncapsulatedDictionaryTests
    {
        [TestMethod]
        public void TestDefaultConstruction()
        {
            var ed = new EncapsulatingDictionary<string, string>();
            Assert.AreEqual( 0, ed.Count );
            Assert.AreEqual( 0, ed.DeepCount );
            Assert.AreEqual( 0, ed.ShallowCount );

            foreach (var _nothing in ed)
                Assert.Fail();

            Assert.AreEqual( 1, ed.Depth );
        }

        [TestMethod]
        public void TestNoEncapsulation()
        {
            var ed = new EncapsulatingDictionary<string, string>();

            ed["name"] = "anne";
            ed["age"] = "8";

            Assert.AreEqual( 2, ed.Count );
            Assert.AreEqual( ed.ShallowCount, ed.DeepCount );
            Assert.AreEqual( 1, ed.Depth );

            Assert.AreEqual( "anne", ed["name"] );
            Assert.AreEqual( "8", ed["age"] );
        }
    }
}
