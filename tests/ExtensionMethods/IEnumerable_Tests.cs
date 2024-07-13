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
        class HasAnInt : IComparable<HasAnInt>
        {
            public int Integer { get; set; }
            public string String { get; set; }
            public override string ToString() => $"[{Integer}]";

            public int CompareTo( HasAnInt other )
            {
                if (CompareString)
                    return String.CompareTo( other.String );
                return Integer.CompareTo( other.Integer );
            }

            public static bool CompareString = false;

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


        [TestMethod]
        public void Smallest_Test()
        {
            var list = new HasAnInt[]
            {
                new HasAnInt{ Integer = 12, String = "Twelve" },
                new HasAnInt{ Integer = 23, String = "Twenty-three" },
                new HasAnInt{ Integer = 34, String = "Thirty-four" },
                new HasAnInt{ Integer = 8, String = "zeight" },
                new HasAnInt{ Integer = 12, String = "Twelve" },
            };

            var smallest = list.Smallest( _x => _x.Integer );
            Assert.AreEqual( "zeight", smallest.String );

            var argmin = list.ArgMin( x => x.Integer );
            Assert.AreEqual( "zeight", list[argmin].String );

            smallest = list.Smallest( x => x.String );
            Assert.AreEqual( 34, smallest.Integer );

            argmin = list.ArgMin( x => x.String );
            Assert.AreEqual( 34, list[argmin].Integer );

            HasAnInt.CompareString = true;
            argmin = list.ArgMin();
            Assert.AreEqual( 34, list[argmin].Integer );

            HasAnInt.CompareString = false;
            argmin = list.ArgMin();
            Assert.AreEqual( "zeight", list[argmin].String );
        }

        [TestMethod]
        public void Largest_Test()
        {
            var list = new HasAnInt[]
            {
                new HasAnInt{ Integer = 12, String = "Twelve" },
                new HasAnInt{ Integer = 23, String = "Twenty-three" },
                new HasAnInt{ Integer = 34, String = "Thirty-four" },
                new HasAnInt{ Integer = 8, String = "zeight" },
                new HasAnInt{ Integer = 12, String = "Twelve" },
            };

            var largest = list.Largest( _x => _x.Integer );
            Assert.AreEqual( "Thirty-four", largest.String );

            var argmax = list.ArgMax( x => x.Integer );
            Assert.AreEqual( "Thirty-four", list[argmax].String );

            largest = list.Largest( x => x.String );
            Assert.AreEqual( 8, largest.Integer );

            argmax = list.ArgMax( x => x.String );
            Assert.AreEqual( 8, list[argmax].Integer );

            HasAnInt.CompareString = true;
            argmax = list.ArgMax();
            Assert.AreEqual( 8, list[argmax].Integer );

            HasAnInt.CompareString = false;
            argmax = list.ArgMax();
            Assert.AreEqual( "Thirty-four", list[argmax].String );
        }

        [TestMethod]
        public void IsEmpty_Test()
        {
            var list = new List<int> { };
            Assert.AreEqual( true, list.IsEmpty() );
            list.Add( 1 );
            Assert.AreEqual( false, list.IsEmpty() );
        }

        [TestMethod]
        public void IsNotEmpty_Test()
        {
            var list = new List<int> { 1 };
            Assert.AreEqual( true, list.IsNotEmpty() );
            list.Clear();
            Assert.AreEqual( false, list.IsNotEmpty() );
        }

        [TestMethod]
        public void SelectWithRegex_Test()
        {
            var list = new[] { "with 7 apples", "with 3 oranges", "with 2 bananas" };
            var result = list.SelectWithRegex( @"\s(\d+)\s" ).Select( int.Parse ).ToList();
            Assert.AreEqual( 3, result.Count );
            Assert.AreEqual( 7, result[0] );
            Assert.AreEqual( 3, result[1] );
            Assert.AreEqual( 2, result[2] );
        }

        [TestMethod]
        public void SelectWithRegexIgnoreExceptions_Test()
        {
            var list = new[] { "with 7 apples", "with 3d oranges", "with 2 bananas" };
            var result = list.SelectWithRegex( @"\s(\d+)\s" ).SelectIgnoreExceptions( int.Parse ).ToList();
            Assert.AreEqual( 2, result.Count );
            Assert.AreEqual( 7, result[0] );
            Assert.AreEqual( 2, result[1] );
        }
    }
}
