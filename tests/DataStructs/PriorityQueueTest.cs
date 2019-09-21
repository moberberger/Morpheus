using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Morpheus.Standard.UnitTests.DataStructs
{
    [TestClass]
    [TestCategory("Data Structures")]
    public class PriorityQueueTest
    {
        #region Data objects for priority queue

        public static readonly int[] INT_DATA = { 5, 1, 9, 6, 3, 8, 10, 11 };

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // A simple object for testing the CPriorityQueue implementations using a variety of options
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        private class CPerson
        {
            public string Name;
            public int Age;
            public override string ToString() => Name + " [" + Age + "]";
        }

        private class CPersonByAge : CPerson, IComparable<CPersonByAge>, IComparable
        {
            public int CompareTo( CPersonByAge other )
            {
                if (other == null)
                    return 1;
                else
                    return Age - other.Age;
            }


            public int CompareTo( object obj ) => CompareTo( obj as CPersonByAge );
        }

        private class CPersonWithIndex : CPersonByAge, IOptimizedBinaryHeapNode
        {
            public int HeapIndex { get; set; }
        }

        private static readonly CPerson[] sm_people = new CPerson[]
            {
                new CPerson(){ Name="Homer", Age=40 },
                new CPerson(){ Name="Lisa", Age=7 },
                new CPerson(){ Name="Bart", Age=10 },
                new CPerson(){ Name="Maggie", Age=3 },
                new CPerson(){ Name="Marge", Age=37 },
                new CPerson(){ Name="Burns", Age=97 },
                new CPerson(){ Name="Smithers", Age=30 },
                new CPerson(){ Name="Nelson", Age=9 },
            };
        private static readonly CPersonByAge[] sm_peopleByAge = new CPersonByAge[]
            {
                new CPersonByAge(){ Name="Homer", Age=40 },
                new CPersonByAge(){ Name="Lisa", Age=7 },
                new CPersonByAge(){ Name="Bart", Age=10 },
                new CPersonByAge(){ Name="Maggie", Age=3 },
                new CPersonByAge(){ Name="Marge", Age=37 },
                new CPersonByAge(){ Name="Burns", Age=97 },
                new CPersonByAge(){ Name="Smithers", Age=30 },
                new CPersonByAge(){ Name="Nelson", Age=9 },
            };
        private static readonly CPersonWithIndex[] sm_peopleWithIndex = new CPersonWithIndex[]
            {
                new CPersonWithIndex(){ Name="Homer", Age=40 },
                new CPersonWithIndex(){ Name="Lisa", Age=7 },
                new CPersonWithIndex(){ Name="Bart", Age=10 },
                new CPersonWithIndex(){ Name="Maggie", Age=3 },
                new CPersonWithIndex(){ Name="Marge", Age=37 },
                new CPersonWithIndex(){ Name="Burns", Age=97 },
                new CPersonWithIndex(){ Name="Smithers", Age=30 },
                new CPersonWithIndex(){ Name="Nelson", Age=9 },
            };

        #endregion


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// These tests are for a priority queue of primitives (ints for these tests)
        /// Some functions/features don't work as expected with value-types, especially
        /// the "Update" features, so they won't be tested.
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void GeneralAcceptanceTest() => GeneralPrimitivesTest( new PriorityQueue<int>( INT_DATA ) );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void EmptyQueueTest() => EmptyPrimitivesTest( new PriorityQueue<int>( INT_DATA ) );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void RemoveTest() => RemovePrimitivesTest( new PriorityQueue<int>( INT_DATA ) );


        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void GeneralAcceptanceExtendedTest() => GeneralPrimitivesTest( new PriorityQueueExtended<int>( INT_DATA ) );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void EmptyQueueExtendedTest() => EmptyPrimitivesTest( new PriorityQueueExtended<int>( INT_DATA ) );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void RemoveExtendedTest() => RemovePrimitivesTest( new PriorityQueueExtended<int>( INT_DATA ) );





        public static void GeneralPrimitivesTest( PriorityQueue<int> q )
        {
            var arr = new int[q.Count];
            q.CopyTo( arr, 0 );

            var realSorted = (int[]) INT_DATA.Clone();
            Array.Sort( realSorted );

            Assert.AreEqual( q.Count, realSorted.Length, "Count wrong" );
            for (var i = 0; i < realSorted.Length; i++)
            {
                Assert.AreEqual( realSorted[i], arr[i], "Incorrect at index " + i );
            }

            var lowest = q.LowestNode;
            Assert.AreEqual( 1, lowest, "Lowest node is wrong" );
            Assert.IsFalse( q.IsReadOnly, "Should not be readonly" );
            Assert.IsTrue( q.Contains( 9 ), "Should contain 9" );
            Assert.IsFalse( q.Contains( -9 ), "Should not contain -9" );

            q.CopyTo( arr, arr.Length + 1 ); // really for code-coverage- make sure there's no error doing this
        }

        public static void EmptyPrimitivesTest( PriorityQueue<int> q )
        {
            Assert.AreEqual( INT_DATA.Length, q.Count, "Count should equal length of source array" );

            q.Clear();
            Assert.AreEqual( 0, q.RemoveLowest(), "Should remove a zero from an empty queue" );
            Assert.AreEqual( 0, q.LowestNode, "Lowest Node should be zero in an empty queue" );
            Assert.AreEqual( 0, q.Count, "Count should be zero" );

            var en = ((System.Collections.IEnumerable) q).GetEnumerator();
            Assert.IsNotNull( en, "Should get an enumerator" );
        }

        public static void RemovePrimitivesTest( PriorityQueue<int> q )
        {
            Assert.AreEqual( INT_DATA.Length, q.Count, "Count incorrect" );

            Assert.IsFalse( q.Remove( -1 ), "Shouldn't be able to remove a -1" );
            Assert.AreEqual( INT_DATA.Length, q.Count, "Count incorrect after non-existent remove" );

            q.Remove( 9 );

            var list = new List<int>( INT_DATA );
            list.RemoveAt( 2 ); // remove the 9
            list.Sort();

            Assert.AreEqual( list.Count, q.Count, "List and Queue lengths should be equal after removal" );

            for (var i = 0; i < q.Count; i++)
            {
                Assert.AreEqual( list[i], q.RemoveLowest(), "Incorrect element at index " + i );
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Test some of the base-class features
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void QueueCapacityTest()
        {
            var buf = new byte[100];
            Rng.Default.NextBytes( buf );
            var list = new List<byte>( 250 );
            list.AddRange( buf );
            Assert.AreEqual( 100, list.Count, "List count wrong" );
            Assert.AreEqual( 250, list.Capacity, "List capacity wrong" );

            var q = new PriorityQueue<byte>( list );
            Assert.AreEqual( 100, q.Count, "Queue count is wrong" );
            Assert.AreEqual( 250, q.Capacity, "Queue capacity is wrong" );

            q.Capacity = 500;
            Assert.AreEqual( 500, q.Capacity, "Queue capacity is wrong after changing it" );
        }





        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Test the Collation Order features of the queue- both using a Comparer and using IComparable
        /////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void IComparableTest() => IComparableTest<CPersonByAge, PriorityQueue<CPersonByAge>>( sm_peopleByAge );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void IComparableExtendedTest() => IComparableTest<CPersonByAge, PriorityQueueExtended<CPersonByAge>>( sm_peopleByAge );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void IComparableOptimizedTest()
        {
            IComparableTest<CPersonWithIndex, PriorityQueueOptimized<CPersonWithIndex>>( sm_peopleWithIndex );
            foreach (var p in sm_peopleWithIndex)
                Assert.AreEqual( -1, p.HeapIndex, "Expected heap index of -1 for " + p.Name );
        }


        private void IComparableTest<T, Q>( T[] _array )
            where Q : PriorityQueue<T>, new()
            where T : CPerson
        {
            var queue = new Q
            {
                Comparer = null // make sure we're using the "default comparer", aka IComparer<T>
            };
            queue.AddRange( _array );
            Assert.AreEqual( _array.Length, queue.Count, "Count of queue doesn't equal length of array" );

            foreach (var p in _array)
                Assert.IsTrue( queue.Contains( p ), "Queue should contain " + p.Name );
            Assert.AreEqual( "Maggie", queue.LowestNode.Name, "Maggie should be the lowest node" );

            Assert.IsFalse( queue.Contains( null ), "Should NOT contain a NULL" );
            Assert.IsFalse( queue.Remove( null ), "Should return FALSE for removing a NULL" );

            foreach (var p in _array.OrderBy( _x => _x.Age ))
            {
                var pp = queue.RemoveLowest();
                Assert.AreEqual( p, pp, "Incorrect order for " + p.Name + " - Removed " + pp.Name );
            }
        }





        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void IComparerTest() => IComparerTest<CPerson, PriorityQueue<CPerson>>( sm_people );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void IComparerExtendedTest() => IComparerTest<CPerson, PriorityQueueExtended<CPerson>>( sm_people );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void IComparerOptimizedTest() => IComparerTest<CPersonWithIndex, PriorityQueueOptimized<CPersonWithIndex>>( sm_peopleWithIndex );



        private void IComparerTest<T, Q>( T[] _array )
            where Q : PriorityQueue<T>, new()
            where T : CPerson
        {
            var queue = new Q();
            queue.SetComparisonFunction( ( _l, _r ) => _r.Name.CompareTo( _l.Name ) );
            queue.AddRange( _array );
            Assert.AreEqual( _array.Length, queue.Count, "Count of NAME queue doesn't equal length of array" );
            foreach (var p in _array)
                Assert.IsTrue( queue.Contains( p ), "Queue should contain " + p.Name );


            foreach (var p in _array.OrderByDescending( _x => _x.Name ))
            {
                var pp = queue.RemoveLowest();
                Assert.AreEqual( p, pp, "Incorrect order for " + p.Name + " - Removed " + pp.Name );
            }

            queue = new Q();
            queue.SetComparisonFunction( ( _l, _r ) => _r.Age - _l.Age );
            queue.AddRange( _array );
            Assert.AreEqual( _array.Length, queue.Count, "Count of AGE queue doesn't equal length of array- Second queue" );
            foreach (var p in _array)
                Assert.IsTrue( queue.Contains( p ), "Queue should contain " + p.Name );


            foreach (var p in _array.OrderByDescending( _x => _x.Age ))
            {
                var pp = queue.RemoveLowest();
                Assert.AreEqual( p, pp, "Incorrect order for " + p.Age + " - Removed " + pp.Age );
            }

        }





        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void UpdateKeyTest() => UpdateKeyTest<CPerson, PriorityQueue<CPerson>>( sm_people );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void UpdateKeyExtendedTest() => UpdateKeyTest<CPerson, PriorityQueueExtended<CPerson>>( sm_people );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void UpdateKeyOptimizedTest() => UpdateKeyTest<CPersonWithIndex, PriorityQueueOptimized<CPersonWithIndex>>( sm_peopleWithIndex );



        private void UpdateKeyTest<T, Q>( T[] _array )
            where Q : PriorityQueue<T>, new()
            where T : CPerson, new()
        {
            var queue = new Q();
            queue.SetComparisonFunction( ( left, right ) => right.Name.CompareTo( left.Name ) );
            queue.AddRange( _array );
            Assert.AreEqual( _array.Length, queue.Count, "Length of queue is wrong" );

            Assert.AreEqual( "Smithers", queue.LowestNode.Name, "Incorrect lowest element for Name reverse-order" );

            var newP = new T() { Name = "AAA", Age = 55 };
            queue.Update( newP ); // should put it at end of queue - Check this after removing all elements from array

            var homer = _array[0];
            homer.Name = "ZHomer";
            queue.Update( homer );
            Assert.AreEqual( "ZHomer", queue.LowestNode.Name, "Incorrect lowest element for Name reverse-order AFTER MODIFICATION" );

            homer.Name = "Homer";
            queue.Update( homer );
            Assert.AreEqual( "Smithers", queue.LowestNode.Name, "Incorrect lowest element AFTER REVERTING HOMER BACK FROM ZHOMER" );

            foreach (var p in _array.OrderByDescending( _x => _x.Name ))
            {
                var pp = queue.RemoveLowest();
                Assert.AreEqual( p.Name, pp.Name, "Incorrect order for " + p.Name + " - Removed " + pp.Name );
                Assert.AreEqual( p.Age, pp.Age, "Incorrect order for " + p.Age + " - Removed " + pp.Age );
            }
            Assert.AreEqual( 1, queue.Count, "The AAA element should still be on the queue" );
            Assert.AreEqual( "AAA", queue.LowestNode.Name, "Incorrect lowest element for Name reverse-order AFTER UPDATE-BASED ADD" );
            queue.RemoveLowest();
            Assert.AreEqual( 0, queue.Count, "After removing AAA, the queue should be empty" );
            Assert.IsNull( queue.RemoveLowest(), "Should return NULL for removing from an empty queue" );

        }


        [TestMethod]
        [TestCategory( "DataStructs" )]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void UpdateNullTest() => UpdateNullTest<CPerson, PriorityQueue<CPerson>>( sm_people );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void UpdateNullExtendedTest() => UpdateNullTest<CPerson, PriorityQueueExtended<CPerson>>( sm_people );

        [TestMethod]
        [TestCategory( "DataStructs" )]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void UpdateNullOptimizedTest() => UpdateNullTest<CPersonWithIndex, PriorityQueueOptimized<CPersonWithIndex>>( sm_peopleWithIndex );



        private void UpdateNullTest<T, Q>( T[] _array )
            where Q : PriorityQueue<T>, new()
            where T : CPerson, new()
        {
            var queue = new Q();
            var person = new T() { Name = "test", Age = 55 };

            queue.Add( person );
            queue.Update( null );
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Test the various versions of the constructor that takes an IEnumerable
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void GeneralIEnumerableTest()
        {
            var q = new PriorityQueue<CPersonWithIndex>( EnumPeople() );
            VerifyGeneralEnumerable( q );
        }

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void GeneralIEnumerableExtendedTest()
        {
            var q = new PriorityQueueExtended<CPersonWithIndex>( EnumPeople() );
            VerifyGeneralEnumerable( q );
        }

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void GeneralIEnumerableOptimizedTest()
        {
            var q = new PriorityQueueOptimized<CPersonWithIndex>( EnumPeople() );
            VerifyGeneralEnumerable( q );
        }


        private void VerifyGeneralEnumerable( PriorityQueue<CPersonWithIndex> q )
        {
            var list = new List<CPersonWithIndex>( sm_peopleWithIndex );
            list.Sort();

            for (var i = 0; i < list.Count; i++)
            {
                Assert.AreEqual( list[i], q.RemoveLowest(), "Person incorrect at index " + i );
            }
        }

        private IEnumerable<CPersonWithIndex> EnumPeople()
        {
            for (var i = 0; i < sm_peopleByAge.Length; i++)
            {
                yield return sm_peopleWithIndex[i];
            }
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Test the various versions of the constructor that takes an IEnumerable
        /////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void TransferToDifferentPQTest()
        {
            var q1 = new PriorityQueueExtended<CPersonWithIndex>( sm_peopleWithIndex );
            var q2 = new PriorityQueueOptimized<CPersonWithIndex>( q1, LambdaComparer<CPersonWithIndex>.FromFunc( ( _l, _r ) => _r.Age - _l.Age ) );

            var arr1 = q1.ToArray();
            var arr2 = q2.ToArray();

            Assert.AreEqual( arr1.Length, arr2.Length, "Array lengths aren't equal" );

            for (var i = 0; i < arr1.Length; i++)
            {
                var j = arr1.Length - i - 1;
                Assert.AreEqual( arr1[i], arr2[j], "The objects are not equal at index " + i + " / " + j );
            }
        }

        [TestMethod]
        [TestCategory( "DataStructs" )]
        public void TransferToExtendedTest()
        {
            var q1 = new PriorityQueueOptimized<CPersonWithIndex>( sm_peopleWithIndex );
            var q2 = new PriorityQueueExtended<CPersonWithIndex>( q1 );

            var arr1 = q1.ToArray();
            var arr2 = q2.ToArray();

            Assert.AreEqual( arr1.Length, arr2.Length, "Array lengths aren't equal" );

            for (var i = 0; i < arr1.Length; i++)
            {
                Assert.AreEqual( arr1[i], arr2[i], "The objects are not equal at index " + i );
            }
        }




    }
}
