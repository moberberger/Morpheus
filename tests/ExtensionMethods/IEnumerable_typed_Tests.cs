using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Morpheus.Standard.UnitTests.IEnumerable
{
    [TestClass]
    [TestCategory( "IEnumerable" )]
    public class IEnumerable_Typed_Tests
    {
        [TestMethod]
        public void Array_Last_Test()
        {
            var arr = new int[] { 1, 2, 3 };
            var x = arr.Last();
            Assert.AreEqual( arr[2], x );
        }

        [TestMethod]
        public void IList_Last_Test()
        {
            var list = new List<int> { 1, 2, 3 };
            var x = list.Last();
            Assert.AreEqual( list[2], x );
        }

        [TestMethod]
        public void IList_RemoveLastItem_Test()
        {
            const int lastItem = 42;
            var list = new List<int> { 1, 2, 3, lastItem };
            int beforeLen = list.Count;
            var x = list.RemoveLastItem();
            Assert.AreEqual( lastItem, x );
            Assert.AreEqual( beforeLen - 1, list.Count );
        }

        [TestMethod]
        public void IList_RemoveFirstItem_Test()
        {
            const int firstItem = 42;
            var list = new List<int> { firstItem, 1, 2, 3 };
            int beforeLen = list.Count;
            var x = list.RemoveFirstItem();
            Assert.AreEqual( firstItem, x );
            Assert.AreEqual( beforeLen - 1, list.Count );
        }

        [TestMethod]
        public void IList_Set_Test()
        {
            var list = new List<int> { 1, 2, 3 };
            Assert.AreEqual( 3, list.Count );

            list.Set( 10, 42 );
            Assert.AreEqual( 11, list.Count );
            Assert.AreEqual( 0, list[9] );
            Assert.AreEqual( 42, list[10] );
        }


        [TestMethod]
        public void IDictionary_GetOrAdd_Test()
        {
            var dict = new Dictionary<int, string>
            {
                { 50, "mike" },
                { 42, "everything" },
            };

            var fifty = dict.GetOrAdd( 50, _x => "fif50" );
            Assert.AreEqual( "mike", fifty );
            Assert.AreEqual( "mike", dict[50] );

            var nine = dict.GetOrAdd( 9, _x => "nine" );
            Assert.AreEqual( "nine", nine );
            Assert.AreEqual( "nine", dict[9] );
        }


    }
}
