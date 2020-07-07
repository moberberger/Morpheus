using Microsoft.VisualStudio.TestTools.UnitTesting;

using Morpheus;

using System.Collections.Generic;

namespace MSUnitTests.DataStructs
{
    [TestClass]
    [TestCategory( "Data Structures" )]
    public class EncapsulatedDictionaryTests
    {
        private EncapsulatingDictionary<string, string> _1, _2, _3;

        [TestInitialize]
        public void SetMembers()
        {
            _1 = new EncapsulatingDictionary<string, string>();
            _2 = new EncapsulatingDictionary<string, string>( _1 );
            _3 = new EncapsulatingDictionary<string, string>( _2 );

            _1["all"] = "1";
            _2["all"] = "2";
            _3["all"] = "3";

            _1["a"] = "1a";
            _1["b"] = "1b";
            _2["b"] = "2b";
            _2["c"] = "2c";
            _3["a"] = "3a";
            _3["c"] = "3c";
        }



        [TestMethod]
        public void DefaultConstructionTest()
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
        public void NoEncapsulationTest()
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

        [TestMethod]
        public void TraceTest()
        {
            var t = new List<IDictionary<string, string>>( _3.TraceKey( "all" ) );
            Assert.AreEqual( 3, t.Count );
            Assert.AreEqual( _3, t[0] );
            Assert.AreEqual( _2, t[1] );
            Assert.AreEqual( _1, t[2] );

            t = new List<IDictionary<string, string>>( _3.TraceKey( "a" ) );
            Assert.AreEqual( 2, t.Count );
            Assert.AreEqual( _3, t[0] );
            Assert.AreEqual( _1, t[1] );

            t = new List<IDictionary<string, string>>( _3.TraceKey( "b" ) );
            Assert.AreEqual( 2, t.Count );
            Assert.AreEqual( _2, t[0] );
            Assert.AreEqual( _1, t[1] );

            t = new List<IDictionary<string, string>>( _3.TraceKey( "c" ) );
            Assert.AreEqual( 2, t.Count );
            Assert.AreEqual( _3, t[0] );
            Assert.AreEqual( _2, t[1] );

            t = new List<IDictionary<string, string>>( _3.TraceKey( "d" ) );
            Assert.AreEqual( 0, t.Count );

            Assert.AreEqual( "1", _1["all"] );
            Assert.AreEqual( "2", _2["all"] );
            Assert.AreEqual( "3", _3["all"] );

            Assert.AreEqual( "3a", _3["a"] );
            Assert.AreEqual( "2b", _3["b"] );
            Assert.AreEqual( "3c", _3["c"] );

            Assert.AreEqual( "1a", _2["a"] );
            Assert.AreEqual( "2b", _2["b"] );
            Assert.AreEqual( "2c", _2["c"] );

            Assert.AreEqual( "1a", _1["a"] );
            Assert.AreEqual( "1b", _1["b"] );
            Assert.IsFalse( _1.ContainsKey( "c" ) );

            Assert.IsTrue( _2.ContainsKey( "a" ) );
            Assert.IsTrue( _2.ContainsKey( "b" ) );
            Assert.IsTrue( _2.ContainsKey( "c" ) );
            Assert.IsFalse( _2.ContainsKeyShallow( "a" ) );
            Assert.IsTrue( _2.ContainsKeyShallow( "b" ) );
            Assert.IsTrue( _2.ContainsKeyShallow( "c" ) );

            Assert.IsTrue( _3.ContainsKey( "a" ) );
            Assert.IsTrue( _3.ContainsKey( "b" ) );
            Assert.IsTrue( _3.ContainsKey( "c" ) );
            Assert.IsTrue( _3.ContainsKeyShallow( "a" ) );
            Assert.IsFalse( _3.ContainsKeyShallow( "b" ) );
            Assert.IsTrue( _3.ContainsKeyShallow( "c" ) );
        }

        [TestMethod]
        [ExpectedException( typeof( KeyNotFoundException ) )]
        public void MissingKeyTest()
        {
            Assert.AreEqual( "1a", _1["a"] );
            Assert.AreEqual( "1b", _1["b"] );
            Assert.AreEqual( null, _1["c"] );
        }



        [TestMethod]
        [ExpectedException( typeof( KeyNotFoundException ) )]
        public void RemoveTest()
        {
            Assert.AreEqual( "3c", _3["c"] );
            Assert.IsTrue( _3.Remove( "c" ) );
            Assert.AreEqual( "2c", _3["c"] );

            Assert.AreEqual( "2b", _3["b"] );
            Assert.IsFalse( _3.Remove( "b" ) );
            Assert.AreEqual( "2b", _3["b"] );
            Assert.IsTrue( _2.Remove( "b" ) );
            Assert.AreEqual( "1b", _3["b"] );
            Assert.IsTrue( _1.Remove( "b" ) );
            Assert.AreEqual( "1b", _3["b"] ); // exception- keynotfound
        }


        [TestMethod]
        public void ChangedSubDictionaryTest()
        {
            Assert.AreEqual( "2b", _3["b"] );
            _2["b"] = "abc";
            Assert.AreEqual( "abc", _3["b"] );
            _2.Remove( "b" );
            Assert.AreEqual( "1b", _3["b"] );
        }

        [TestMethod]
        public void CopyToTest()
        {
            var count = _3.Count;
            Assert.AreEqual( 4, count );

            KeyValuePair<string, string>[] arr = new KeyValuePair<string, string>[count + 1];

            _3.CopyTo( arr, 1 );
            Assert.IsNull( arr[0].Key );
            Assert.IsNotNull( arr[1].Key );
            Assert.IsNotNull( arr[2].Key );
            Assert.IsNotNull( arr[3].Key );
            Assert.IsNotNull( arr[4].Key );
        }


    }
}
