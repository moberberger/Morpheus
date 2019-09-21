using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Morpheus.Standard.UnitTests.DataStructs
{
    [TestClass]
    [TestCategory( "Data Structures" )]
    public class OrderedListTests
    {
        [TestMethod]
        public void ConstructionTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7 } );
            Assert.AreEqual( 3, list.Count );

            Assert.AreEqual( 5, list[0] );
            Assert.AreEqual( 7, list[1] );
            Assert.AreEqual( 10, list[2] );
        }


        [TestMethod]
        public void ConstructionCapacityTest()
        {
            var list = new OrderedList<int>( 42 );
            Assert.AreEqual( 0, list.Count );
            Assert.AreEqual( 42, list.Capacity );
        }


        [TestMethod]
        public void SingleAddAndRemoveTest()
        {
            var list = new OrderedList<int>();

            list.Add( 42 );
            Assert.AreEqual( 1, list.Count );
            Assert.AreEqual( 42, list[0] );

            bool success = list.Remove( 42 );
            Assert.IsTrue( success );
            Assert.AreEqual( 0, list.Count );

            success = list.Remove( 42 );
            Assert.IsFalse( success );
            Assert.AreEqual( 0, list.Count );
        }

        [TestMethod]
        public void AddTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7 } );
            list.Add( 2 );
            list.Add( 8 );

            Assert.AreEqual( 5, list.Count );

            Assert.AreEqual( 2, list[0] );
            Assert.AreEqual( 5, list[1] );
            Assert.AreEqual( 7, list[2] );
            Assert.AreEqual( 8, list[3] );
            Assert.AreEqual( 10, list[4] );
        }

        [TestMethod]
        public void AddDuplicateTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7 } );
            list.Add( 7 );

            Assert.AreEqual( 4, list.Count );

            Assert.AreEqual( 5, list[0] );
            Assert.AreEqual( 7, list[1] );
            Assert.AreEqual( 7, list[2] );
            Assert.AreEqual( 10, list[3] );
        }


        [TestMethod]
        public void IndexOfTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7, 5, 2, 15 } );

            Assert.AreEqual( 6, list.Count );

            var idx = list.IndexOf( 5 );
            var pass = idx == 1 || idx == 2;
            Assert.IsTrue( pass );
        }


        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void FailItemIndexSetTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7, 5, 2, 15 } );
            list[0] = 33;
        }


        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void FailInsertSetTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7, 5, 2, 15 } );
            list.Insert( 0, 99 );
        }


        [TestMethod]
        public void ToArrayTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7 } );
            var arr = list.ToArray();

            Assert.AreEqual( 3, arr.Length );
            Assert.AreEqual( 5, arr[0] );
            Assert.AreEqual( 7, arr[1] );
            Assert.AreEqual( 10, arr[2] );
        }

        [TestMethod]
        public void IEnumerableTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7 } );
            var sum = list.Sum();
            Assert.AreEqual( 22, sum );
        }


        [TestMethod]
        public void ContainsTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7, 5, 2, 15 } );
            Assert.IsTrue( list.Contains( 7 ) );
            Assert.IsTrue( list.Contains( 5 ) );
            Assert.IsFalse( list.Contains( 42 ) );
        }

        [TestMethod]
        public void ClearTest()
        {
            var list = new OrderedList<int>( new int[] { 10, 5, 7, 5, 2, 15 } );
            Assert.AreEqual( 6, list.Count );

            list.Clear();
            Assert.AreEqual( 0, list.Count );

            Assert.IsFalse( list.IsReadOnly );
        }

    }
}
