using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Morpheus.Standard.UnitTests.IEnumerable
{
    [TestClass]
    [TestCategory( "IEnumerable" )]
    public class IEnumerable_Tests
    {
        class HasAnInt
        {
            public int Integer { get; set; }
            public override string ToString() => $"[{Integer}]";
        }

        [TestMethod]
        public void Object_JoinAsString_Test()
        {
            var list = new HasAnInt[]
            {
                new HasAnInt{ Integer = 42 },
                new HasAnInt{ Integer = 23 },
            };

            var joined = list.JoinAsString( " " );
            Assert.AreEqual( "[42] [23]", joined );
        }

        [TestMethod]
        public void Object_JoinAsString_stringizer_Test()
        {
            var list = new HasAnInt[]
            {
                new HasAnInt{ Integer = 42 },
                new HasAnInt{ Integer = 23 },
            };

            var joined = list.JoinAsString( ",", _obj => $"({_obj.Integer * 2})" );
            Assert.AreEqual( "(84),(46)", joined );
        }

        [TestMethod]
        public void FirstIndexOf_Test()
        {
            var list = new HasAnInt[]
            {
                new HasAnInt{ Integer = 12 },
                new HasAnInt{ Integer = 23 },
                new HasAnInt{ Integer = 34 },
                new HasAnInt{ Integer = 23 },
                new HasAnInt{ Integer = 12 },
            };

            int x = list.FirstIndexOf( _x => _x.Integer == 23 );
            Assert.AreEqual( 1, x );
        }

        [TestMethod]
        public void FirstIndexOf_miss_Test()
        {
            var list = new HasAnInt[]
            {
                new HasAnInt{ Integer = 12 },
                new HasAnInt{ Integer = 23 },
                new HasAnInt{ Integer = 34 },
                new HasAnInt{ Integer = 23 },
                new HasAnInt{ Integer = 12 },
            };

            int x = list.FirstIndexOf( _x => _x.Integer == 234 );
            Assert.AreEqual( -1, x );
        }
    }
}
