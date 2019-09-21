using Microsoft.VisualStudio.TestTools.UnitTesting;
using Morpheus.Serialization;
using System;
using System.Collections;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    [TestClass]
    public class CSystemCollectionsTest : CBaseXmlPrinter
    {
        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestAllSortedLists()
        {
            var ca = new CClassWithSortedLists
            {
                SimpleSortedList = MakeSampleSortedList( false )
            };

            var tmp = MakeSampleSortedList( true );
            ca.SyncSortedList = SortedList.Synchronized( tmp );

            Assert.AreNotEqual( ca.SimpleSortedList.GetType(),
                                ca.SyncSortedList.GetType(),
                                "The Types of the sortedLists should be different." );

            Console.WriteLine( ca.SimpleSortedList.GetType().AssemblyQualifiedName );
            Console.WriteLine( ca.SyncSortedList.GetType().AssemblyQualifiedName );

            var s = new CSerializer();
            var doc = s.Serialize( ca );

            Print( doc );

            var d = new CDeserializer();
            var ca2 = d.Deserialize<CClassWithSortedLists>( doc );

            Assert.AreEqual( ca.SimpleSortedList.GetType(),
                             ca2.SimpleSortedList.GetType(),
                             "The Type of the resulting SortedList is different." );
            Assert.AreEqual( ca.SyncSortedList.GetType(), ca2.SyncSortedList.GetType(), "The Type of the Sync SortedList is different" );

            Assert.AreNotEqual( ca2.SimpleSortedList.GetType(),
                                ca2.SyncSortedList.GetType(),
                                "The Type of the Simple SortedList is the same as the Sync'ed one" );

            VerifySortedListContents( ca.SimpleSortedList, ca2.SimpleSortedList );
            VerifySortedListContents( ca.SyncSortedList, ca2.SyncSortedList );
        }

        private SortedList MakeSampleSortedList( bool _doCaps )
        {
            var list = new SortedList();
            var numbers = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

            for (var i = 0; i < numbers.Length; i++)
            {
                list.Add( _doCaps ? numbers[i].ToUpper() : numbers[i], i );
            }

            return list;
        }

        private void VerifySortedListContents( SortedList s1, SortedList s2 )
        {
            Assert.AreEqual( s1.Count, s2.Count, "The sizes of the two lists is different" );
            foreach (var key in s1.Keys)
            {
                Assert.AreEqual( s1[key], s2[key], "Values differ for key: " + key.ToString() );
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestAllHashtables()
        {
            var ca = new CClassWithHashtables
            {
                SimpleHashtable = MakeSampleHashtable()
            };
            var tmp = MakeSampleHashtable2();
            ca.SyncHashtable = Hashtable.Synchronized( tmp );

            Assert.AreNotEqual( ca.SimpleHashtable.GetType(),
                                ca.SyncHashtable.GetType(),
                                "The Types of the hashtables should be different." );

            Console.WriteLine( ca.SimpleHashtable.GetType().AssemblyQualifiedName );
            Console.WriteLine( ca.SyncHashtable.GetType().AssemblyQualifiedName );

            var s = new CSerializer();
            var doc = s.Serialize( ca );

            Print( doc );

            var d = new CDeserializer();
            var ca2 = d.Deserialize<CClassWithHashtables>( doc );

            Assert.AreEqual( ca.SimpleHashtable.GetType(),
                             ca2.SimpleHashtable.GetType(),
                             "The Type of the resulting Hashtable is different." );
            Assert.AreEqual( ca.SyncHashtable.GetType(), ca2.SyncHashtable.GetType(), "The Type of the Sync Hashtable is different" );

            Assert.AreNotEqual( ca2.SimpleHashtable.GetType(),
                                ca2.SyncHashtable.GetType(),
                                "The Type of the Simple Hashtable is the same as the Sync'ed one" );

            VerifyHashtableContents( ca.SimpleHashtable, ca2.SimpleHashtable );
            VerifyHashtableContents( ca.SyncHashtable, ca2.SyncHashtable );
        }

        private Hashtable MakeSampleHashtable()
        {
            var h = new Hashtable
            {
                ["Name"] = "Homer",
                ["City"] = "Springfield",
                ["Age"] = 35,
                ["Birthdate"] = new DateTime( 1974, 4, 17, 23, 11, 9 ),
                ["Rating"] = 8.25
            };

            return h;
        }

        private Hashtable MakeSampleHashtable2()
        {
            var h = new Hashtable
            {
                ["Name"] = "Lisa",
                ["City"] = "Boston",
                ["Age"] = 7,
                ["Birthdate"] = new DateTime( 1994, 4, 17, 23, 11, 9 ),
                ["Rating"] = 4.25
            };

            return h;
        }

        private void VerifyHashtableContents( Hashtable h1, Hashtable h2 )
        {
            Assert.AreEqual( h1.Count, h2.Count, "The sizes of the two hashtables is different" );
            foreach (var key in h1.Keys)
            {
                Assert.AreEqual( h1[key], h2[key], "Values differ for key: " + key.ToString() );
            }
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestAllStacks()
        {
            var ca = new CClassWithStacks
            {
                SimpleStack = MakeSampleStack()
            };
            var tmp = MakeSampleStack();
            ca.SyncStack = Stack.Synchronized( tmp );

            Assert.AreNotEqual( ca.SimpleStack.GetType(),
                                ca.SyncStack.GetType(),
                                "The Type of the Simple Stack is the same as the Sync'ed queue" );

            Console.WriteLine( ca.SimpleStack.GetType().AssemblyQualifiedName );
            Console.WriteLine( ca.SyncStack.GetType().AssemblyQualifiedName );

            var s = new CSerializer();
            var doc = s.Serialize( ca );

            Print( doc );

            var d = new CDeserializer();
            var ca2 = d.Deserialize<CClassWithStacks>( doc );

            Assert.AreEqual( ca.SimpleStack.GetType(),
                             ca2.SimpleStack.GetType(),
                             "The Type of the resulting Stack is different." );
            Assert.AreEqual( ca.SyncStack.GetType(), ca2.SyncStack.GetType(), "The Type of the Sync Stack is different" );

            Assert.AreNotEqual( ca2.SimpleStack.GetType(),
                                ca2.SyncStack.GetType(),
                                "The Type of the Simple Stack is the same as the Sync'ed queue" );

            VerifyStackContents( ca.SimpleStack, ca2.SimpleStack );
            VerifyStackContents( ca.SyncStack, ca2.SyncStack );
        }

        private static Stack MakeSampleStack()
        {
            var d = DateTime.Now;

            var stk = new Stack();
            stk.Push( "This" );
            stk.Push( "is" );
            stk.Push( 45 );
            stk.Push( 8.25 );
            stk.Push( new DateTime( d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second ) );

            return stk;
        }

        private void VerifyStackContents( Stack s1, Stack s2 )
        {
            Assert.AreEqual( s1.Count, s2.Count, "The size of the 2 Stacks are not the same" );
            for (var i = 0; i < s1.Count; i++)
            {
                Assert.AreEqual( s1.Pop(), s2.Pop(), "The objects were wrong at index: " + i );
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestAllQueues()
        {
            var ca = new CClassWithQueues
            {
                SimpleQueue = MakeSampleQueue()
            };
            var tmp = MakeSampleQueue();
            ca.SyncQueue = Queue.Synchronized( tmp );

            Assert.AreNotEqual( ca.SimpleQueue.GetType(),
                                ca.SyncQueue.GetType(),
                                "The Type of the Simple Queue is the same as the Sync'ed queue" );

            Console.WriteLine( ca.SimpleQueue.GetType().AssemblyQualifiedName );
            Console.WriteLine( ca.SyncQueue.GetType().AssemblyQualifiedName );

            var s = new CSerializer();
            var doc = s.Serialize( ca );

            Print( doc );

            var d = new CDeserializer();
            var ca2 = d.Deserialize<CClassWithQueues>( doc );

            Assert.AreEqual( ca.SimpleQueue.GetType(),
                             ca2.SimpleQueue.GetType(),
                             "The Type of the resulting Queue is different." );
            Assert.AreEqual( ca.SyncQueue.GetType(), ca2.SyncQueue.GetType(), "The Type of the Sync Queue is different" );

            Assert.AreNotEqual( ca2.SimpleQueue.GetType(),
                                ca2.SyncQueue.GetType(),
                                "The Type of the Simple Queue is the same as the Sync'ed queue" );

            VerifyQueueContents( ca.SimpleQueue, ca2.SimpleQueue );
            VerifyQueueContents( ca.SyncQueue, ca2.SyncQueue );
        }

        private static Queue MakeSampleQueue()
        {
            var d = DateTime.Now;

            var q = new Queue();
            q.Enqueue( "This" );
            q.Enqueue( "is" );
            q.Enqueue( 45 );
            q.Enqueue( 8.25 );
            q.Enqueue( new DateTime( d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second ) );

            return q;
        }

        private void VerifyQueueContents( Queue q1, Queue q2 )
        {
            Assert.AreEqual( q1.Count, q2.Count, "The size of the 2 Queues are not the same" );
            for (var i = 0; i < q1.Count; i++)
            {
                Assert.AreEqual( q1.Dequeue(), q2.Dequeue(), "The objects were wrong at index: " + i );
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestAllArrayLists()
        {
            var ca = new CClassWithArrayLists
            {
                SimpleArrayList = MakeSampleArrayList()
            };
            ca.ReadOnlyArrayList = ArrayList.ReadOnly( ca.SimpleArrayList );
            // This is really bad because we shouldn't really hold the ref to the "old" object after its been wrapped
            ca.SyncArrayList = ArrayList.Synchronized( ca.SimpleArrayList );
            // This is really bad because we shouldn't really hold the ref to the "old" object after its been wrapped

            Assert.AreNotEqual( ca.SimpleArrayList.GetType(),
                                ca.ReadOnlyArrayList.GetType(),
                                "The Type of the ArrayList is the SAME as the ReadOnly ArrayList." );
            Assert.AreNotEqual( ca.SimpleArrayList.GetType(),
                                ca.SyncArrayList.GetType(),
                                "The Type of the ArrayList is the SAME as the SyncArrayList." );
            Assert.AreNotEqual( ca.SyncArrayList.GetType(),
                                ca.ReadOnlyArrayList.GetType(),
                                "The Type of the SyncArrayList is the SAME as the ReadOnly ArrayList." );

            Console.WriteLine( ca.SimpleArrayList.GetType().AssemblyQualifiedName );
            Console.WriteLine( ca.ReadOnlyArrayList.GetType().AssemblyQualifiedName );
            Console.WriteLine( ca.SyncArrayList.GetType().AssemblyQualifiedName );

            CSerializationContext.Global.UseFullUtcDateTimeStrings = true;
            var s = new CSerializer();
            var doc = s.Serialize( ca );

            Print( doc );

            var d = new CDeserializer();
            var ca2 = d.Deserialize<CClassWithArrayLists>( doc );

            Assert.AreEqual( ca.SimpleArrayList.GetType(),
                             ca2.SimpleArrayList.GetType(),
                             "The Type of the resulting ArrayList is different." );
            Assert.AreEqual( ca.ReadOnlyArrayList.GetType(),
                             ca2.ReadOnlyArrayList.GetType(),
                             "The Type of the ReadOnly array is different" );
            Assert.AreEqual( ca.SyncArrayList.GetType(),
                             ca2.SyncArrayList.GetType(),
                             "The Type of the Sync array is different" );

            Assert.AreNotEqual( ca2.SimpleArrayList.GetType(),
                                ca2.ReadOnlyArrayList.GetType(),
                                "The Type of the ArrayList is the SAME as the ReadOnly ArrayList." );
            Assert.AreNotEqual( ca2.SimpleArrayList.GetType(),
                                ca2.SyncArrayList.GetType(),
                                "The Type of the ArrayList is the SAME as the SyncArrayList." );
            Assert.AreNotEqual( ca2.SyncArrayList.GetType(),
                                ca2.ReadOnlyArrayList.GetType(),
                                "The Type of the SyncArrayList is the SAME as the ReadOnly ArrayList." );

            VerifyArrayListContents( ca.SimpleArrayList, ca2.SimpleArrayList );
            VerifyArrayListContents( ca.ReadOnlyArrayList, ca2.ReadOnlyArrayList );
            VerifyArrayListContents( ca.SyncArrayList, ca2.SyncArrayList );
        }

        private static ArrayList MakeSampleArrayList()
        {
            var d = DateTime.Now;

            var arr = new ArrayList
            {
                "This",
                "is",
                45,
                8.25,
                new DateTime( d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second )
            };
            return arr;
        }

        private static void VerifyArrayListContents( ArrayList arr, ArrayList arr2 )
        {
            Assert.AreEqual( arr.Count, arr2.Count, "The count of the new arraylist does not match" );
            for (var i = 0; i < arr.Count; i++)
            {
                Assert.AreEqual( arr[i], arr2[i], "The element is incorrect at index: " + i );
            }
        }
    }
}
