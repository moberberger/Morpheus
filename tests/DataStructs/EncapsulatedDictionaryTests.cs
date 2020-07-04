using Microsoft.VisualStudio.TestTools.UnitTesting;

using Morpheus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
            ed["age"] = "82";

            Assert.AreEqual( 2, ed.Count );
            Assert.AreEqual( ed.ShallowCount, ed.DeepCount );
            Assert.AreEqual( 1, ed.Depth );

            Assert.AreEqual( "anne", ed["name"] );
            Assert.AreEqual( "82", ed["age"] );

            ed.Add( "city", "syrupville" );
            Assert.AreEqual( 3, ed.Count );
            Assert.AreEqual( ed.ShallowCount, ed.DeepCount );
            Assert.AreEqual( 1, ed.Depth );
            Assert.IsTrue( ed.ContainsKey( "name" ) );
            Assert.IsTrue( ed.ContainsKey( "age" ) );
            Assert.IsTrue( ed.ContainsKey( "city" ) );
            Assert.IsFalse( ed.ContainsKey( "state" ) );


            Assert.IsFalse( ed.Remove( "state" ) );
            Assert.AreEqual( 3, ed.Count );
            Assert.AreEqual( ed.ShallowCount, ed.DeepCount );
            Assert.AreEqual( 1, ed.Depth );
            
            Assert.IsTrue( ed.Remove( "age" ) );
            Assert.AreEqual( 2, ed.Count );
            Assert.AreEqual( ed.ShallowCount, ed.DeepCount );
            Assert.AreEqual( 1, ed.Depth );


            ed.Clear();
            Assert.AreEqual( 0, ed.Count );
            Assert.AreEqual( ed.ShallowCount, ed.DeepCount );
            Assert.AreEqual( 1, ed.Depth );


        }
    }
}
