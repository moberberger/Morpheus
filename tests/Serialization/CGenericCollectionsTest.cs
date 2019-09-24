using Microsoft.VisualStudio.TestTools.UnitTesting;
using Morpheus.Serialization;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    [TestClass]
    public class CGenericCollectionsTest : CBaseXmlPrinter
    {
        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSortedList()
        {
            var names = GenerateRandomNames();
            var dict = new SortedList<string, CAddress>();

            for (var i = 0; i < 26; i++)
            {
                dict[names[i]] = CAddress.Get();
            }

            var s = new CSerializer();
            var doc = s.Serialize( dict );

            Print( doc );

            var d = new CDeserializer();
            var d2 = (SortedList<string, CAddress>) d.Deserialize( doc );

            Assert.AreEqual( dict.Count, d2.Count, "Size of resulting dictionary is wrong" );
            Assert.AreEqual( dict.Count, doc.DocumentElement.ChildNodes.Count, "The number of XmlNodes for the collection is wrong" );

            foreach (var key in dict.Keys)
            {
                CompareCAddresses( dict[key], d2[key] );
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSortedDictionary()
        {
            var names = GenerateRandomNames();
            var dict = new SortedDictionary<string, CAddress>();

            for (var i = 0; i < 26; i++)
            {
                dict[names[i]] = CAddress.Get();
            }

            var s = new CSerializer();
            var doc = s.Serialize( dict );

            Print( doc );

            var d = new CDeserializer();
            var d2 = (SortedDictionary<string, CAddress>) d.Deserialize( doc );

            Assert.AreEqual( dict.Count, d2.Count, "Size of resulting dictionary is wrong" );
            Assert.AreEqual( dict.Count, doc.DocumentElement.ChildNodes.Count, "The number of XmlNodes for the collection is wrong" );

            foreach (var key in dict.Keys)
            {
                CompareCAddresses( dict[key], d2[key] );
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestStack()
        {
            var q = new Stack<CAddress>();

            for (var i = 1; i < 11; i++)
            {
                q.Push( CAddress.Get() );
            }

            var s = new CSerializer();
            var doc = s.Serialize( q );

            Print( doc );

            var d = new CDeserializer();
            var q2 = (Stack<CAddress>) d.Deserialize( doc );

            Assert.AreEqual( q.Count, q2.Count, "Number of resulting elements is wrong." );
            Assert.AreEqual( q.Count,
                             doc.DocumentElement.ChildNodes.Count,
                             "The number of child nodes does not equal the number of elements in the Collection." );

            while (q.Count > 0)
            {
                var a1 = q.Pop();
                var a2 = q2.Pop();
                CompareCAddresses( a1, a2 );
            }
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestQueue()
        {
            var q = new Queue<CAddress>();

            for (var i = 1; i < 15; i++)
            {
                q.Enqueue( CAddress.Get() );
            }

            var s = new CSerializer();
            var doc = s.Serialize( q );

            Print( doc );

            var d = new CDeserializer();
            var q2 = (Queue<CAddress>) d.Deserialize( doc );

            Assert.AreEqual( q.Count, q2.Count, "Number of resulting elements is wrong." );
            Assert.AreEqual( q.Count,
                             doc.DocumentElement.ChildNodes.Count,
                             "The number of child nodes does not equal the number of elements in the Collection." );

            while (q.Count > 0)
            {
                var a1 = q.Dequeue();
                var a2 = q2.Dequeue();
                CompareCAddresses( a1, a2 );
            }
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestLinkedList()
        {
            var list = new LinkedList<int>();

            for (var i = 1; i < 1000000; i = (int) ((i + 1) * 1.5))
            {
                list.AddLast( i );
            }

            var s = new CSerializer();
            var doc = s.Serialize( list );

            Print( doc );

            var d = new CDeserializer();
            var list2 = d.Deserialize( doc ) as LinkedList<int>;

            Assert.AreEqual( list.Count, list2.Count, "Number of resulting elements is wrong." );

            var ptr = list2.First;
            foreach (var x in list)
            {
                Assert.AreEqual( x, ptr.Value, "The deserialized value is wrong for initial value: " + x );
                ptr = ptr.Next;
            }
        }


        private static readonly Random sm_rng = new Random( 876 );

        private static string[] GenerateRandomNames()
        {
            var names = new string[26];
            for (var i = 0; i < 26; i++)
            {
                names[i] = new string( (char) (i + 'A'), 5 );
            }

            for (var i = 0; i < 26; i++)
            {
                var x = sm_rng.Next( 26 );
                var tmp = names[x];
                names[x] = names[i];
                names[i] = tmp;
            }
            return names;
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestBothInterfaceAndFullSerialization()
        {
            var ca = new CClassWithBothTypesOfCollections();
            for (var i = 0; i < 8; i++)
            {
                var name = "Address:" + i;
                ca.ByName[name] = CAddress.Get();
            }
            for (var i = 0; i < 6; i++)
            {
                ca.AsStack.Push( CAddress.Get() );
            }


            var ctx = new CSerializationContext();
            ctx.SetConcise();
            ctx.RemoveNullValuesFromXml = false; // required for one test that checks the # of child elements where some of them are null

            var s = new CSerializer( ctx );
            var doc = s.Serialize( ca );

            Print( doc );

            var dict = doc.DocumentElement["ByName"];
            Assert.AreEqual( ca.ByName.Count,
                             dict.ChildNodes.Count,
                             "Child Node Count should match Dictionary Count when using an Interface surrogate" );

            var stak = doc.DocumentElement["AsStack"];
            Assert.AreEqual( 4, stak.ChildNodes.Count, "The Stack should have 4 elements in it" );

            var stakCount = stak["_size"];
            Assert.AreEqual( int.Parse( stakCount.InnerText ), ca.AsStack.Count, "The number of stack elements is wrong" );

            var stakElems = stak["_array"];
            var c = int.Parse( XmlExtensions.GetAttributeValue( stakElems, ctx.ArrayAttributeName ) );
            Assert.AreEqual( c, stakElems.ChildNodes.Count, "The number of array elements is wrong." );


            var d = new CDeserializer( ctx );
            var cb = (CClassWithBothTypesOfCollections) d.Deserialize( doc );

            foreach (var key in ca.ByName.Keys)
            {
                Assert.AreEqual( ca.ByName[key].m_street, cb.ByName[key].m_street, "Incorrect Address for key: " + key );
                Assert.AreEqual( ca.ByName[key].m_city, cb.ByName[key].m_city, "Incorrect Address for key: " + key );
                Assert.AreEqual( ca.ByName[key].m_zip, cb.ByName[key].m_zip, "Incorrect Address for key: " + key );
            }

            var s_a = ca.AsStack.ToArray();
            var s_b = cb.AsStack.ToArray();
            Assert.AreEqual( s_a.Length, s_b.Length, "Lengths of Stacks are wrong" );

            for (var i = 0; i < s_a.Length; i++)
            {
                var aa = s_a[i] as CAddress;
                var ab = s_b[i] as CAddress;

                CompareCAddresses( aa, ab );
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestHandwrittenList()
        {
            var xml =
                @"
<_>
    <Name>Homer</Name>
    <Addresses>
        <_><Street>Schroeder Way</Street><City>Sparks</City><Zip>89431</Zip></_>
        <_><Street>Shadow Lane</Street><City>Sparks</City><Zip>89434</Zip></_>
        <_><Street>Lynnfield Court</Street><City>Reno</City><Zip>89509</Zip></_>
        <_ type='Morpheus.Standard.UnitTests.Serialization.CSuperAddress'><Country>Australia</Country><Street>Coast Ave</Street><City>Cronulla</City><Zip>2020</Zip></_>
        <_><Street>Plateau Road</Street><City>Reno</City><Zip>89519</Zip></_>
    </Addresses>
</_>";
            var doc = new XmlDocument();
            doc.LoadXml( xml );
            Print( doc );

            var c = new CSerializationContext
            {
                TypeAttributeName = "type",
                FixM_ = true,
                ArrayElementName = "_"
            };
            var d = new CDeserializer( c );

            var cwl = d.Deserialize<CClassWithIList>( doc );

            Assert.AreEqual( "Homer", cwl.Name, "Name is wrong" );
            Assert.AreEqual( 5, cwl.Addresses.Count, "Number of addresses is wrong" );

            Assert.AreEqual( "Schroeder Way", cwl.Addresses[0].m_street, "[0]- Street" );
            Assert.AreEqual( "Sparks", cwl.Addresses[0].m_city, "[0]- City" );
            Assert.AreEqual( 89431, cwl.Addresses[0].m_zip, "[0]- Zip" );

            Assert.AreEqual( "Shadow Lane", cwl.Addresses[1].m_street, "[1]- Street" );
            Assert.AreEqual( "Sparks", cwl.Addresses[1].m_city, "[1]- City" );
            Assert.AreEqual( 89434, cwl.Addresses[1].m_zip, "[1]- Zip" );

            Assert.AreEqual( "Lynnfield Court", cwl.Addresses[2].m_street, "[2]- Street" );
            Assert.AreEqual( "Reno", cwl.Addresses[2].m_city, "[2]- City" );
            Assert.AreEqual( 89509, cwl.Addresses[2].m_zip, "[2]- Zip" );

            Assert.AreEqual( "Coast Ave", cwl.Addresses[3].m_street, "[3]- Street" );
            Assert.AreEqual( "Cronulla", cwl.Addresses[3].m_city, "[3]- City" );
            Assert.AreEqual( 2020, cwl.Addresses[3].m_zip, "[3]- Zip" );
            var sa = (CSuperAddress) cwl.Addresses[3];
            Assert.AreEqual( "Australia", sa.m_country, "[3]- Country" );

            Assert.AreEqual( "Plateau Road", cwl.Addresses[4].m_street, "[4]- Street" );
            Assert.AreEqual( "Reno", cwl.Addresses[4].m_city, "[4]- City" );
            Assert.AreEqual( 89519, cwl.Addresses[4].m_zip, "[4]- Zip" );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestList()
        {
            var names = new List<string>
            {
                "Homer",
                "Marge",
                "Bart",
                "Lisa",
                "Maggie"
            };

            var c = new CSerializationContext();
            c.SetVerbose();
            var s = new CSerializer( c );
            var doc = s.Serialize( names );

            Print( doc );

            var d = new CDeserializer( c );
            var names2 = d.Deserialize<List<string>>( doc );

            Assert.AreEqual( names.Count, names2.Count, "The number of list elements is wrong" );
            for (var i = 0; i < names.Count; i++)
            {
                Assert.AreEqual( names[i], names2[i], "The name is wrong at index " + i );
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestDictionary()
        {
            var x = new Dictionary<int, string>
            {
                [4] = "hello",
                [55] = "Katie",
                [15834] = "=)",
                [324] = "Homer",
                [-87] = "Simpson"
            };

            var c = new CSerializationContext();
            c.SetConcise();
            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Print( doc );

            var d = new CDeserializer( c );
            var y = (Dictionary<int, string>) d.Deserialize( doc );


            Assert.AreEqual( x.Count, y.Count, "Size of resulting hashtable is wrong" );
            foreach (var key in x.Keys)
            {
                Assert.AreEqual( x[key], y[key], "Entry at key " + key + " was wrong." );
            }
        }


#if false
        [TestMethod][TestCategory( "Serialization" )]
        public void TestAllHashtables()
        {
            CClassWithHashtables ca = new CClassWithHashtables();
            ca.SimpleHashtable = MakeSampleHashtable();
            Hashtable tmp = MakeSampleHashtable2();
            ca.SyncHashtable = Hashtable.Synchronized( tmp );

            Assert.AreNotEqual( ca.SimpleHashtable.GetType(),
                                ca.SyncHashtable.GetType(),
                                "The Types of the hashtables should be different." );

            Console.WriteLine( ca.SimpleHashtable.GetType().AssemblyQualifiedName );
            Console.WriteLine( ca.SyncHashtable.GetType().AssemblyQualifiedName );

            CSerializer s = new CSerializer();
            XmlDocument doc = s.Serialize( ca );

            Print( doc );

            CDeserializer d = new CDeserializer();
            CClassWithHashtables ca2 = d.Deserialize<CClassWithHashtables>( doc );

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
            Hashtable h = new Hashtable();
            h["Name"] = "Homer";
            h["City"] = "Springfield";
            h["Age"] = 35;
            h["Birthdate"] = new DateTime( 1974, 4, 17, 23, 11, 9 );
            h["Rating"] = 8.25;

            return h;
        }

        private Hashtable MakeSampleHashtable2()
        {
            Hashtable h = new Hashtable();
            h["Name"] = "Lisa";
            h["City"] = "Boston";
            h["Age"] = 7;
            h["Birthdate"] = new DateTime( 1994, 4, 17, 23, 11, 9 );
            h["Rating"] = 4.25;

            return h;
        }

        private void VerifyHashtableContents( Hashtable h1, Hashtable h2 )
        {
            Assert.AreEqual( h1.Count, h2.Count, "The sizes of the two hashtables is different" );
            foreach (object key in h1.Keys)
            {
                Assert.AreEqual( h1[key], h2[key], "Values differ for key: " + key.ToString() );
            }
        }


        [TestMethod][TestCategory( "Serialization" )]
        public void TestAllArrayLists()
        {
            CClassWithArrayLists ca = new CClassWithArrayLists();

            ca.SimpleArrayList = MakeSampleArrayList();
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
            CSerializer s = new CSerializer();
            XmlDocument doc = s.Serialize( ca );

            Print( doc );

            CDeserializer d = new CDeserializer();
            CClassWithArrayLists ca2 = d.Deserialize<CClassWithArrayLists>( doc );

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
            DateTime d = DateTime.Now;

            ArrayList arr = new ArrayList();
            arr.Add( "This" );
            arr.Add( "is" );
            arr.Add( 45 );
            arr.Add( 8.25 );
            arr.Add( new DateTime( d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second ) );
            return arr;
        }

        private static void VerifyArrayListContents( ArrayList arr, ArrayList arr2 )
        {
            Assert.AreEqual( arr.Count, arr2.Count, "The count of the new arraylist does not match" );
            for (int i = 0; i < arr.Count; i++)
            {
                Assert.AreEqual( arr[i], arr2[i], "The element is incorrect at index: " + i );
            }
        }


#endif

        private static void CompareCAddresses( CAddress _a1, CAddress _a2 )
        {
            Assert.AreEqual( _a1.m_street, _a2.m_street, "Street wrong" );
            Assert.AreEqual( _a1.m_city, _a2.m_city, "City wrong" );
            Assert.AreEqual( _a1.m_zip, _a2.m_zip, "Zip wrong" );
        }


        [TestInitialize]
        public void ClearContext() => CSerializationContext.Global.ResetToGlobalDefault();
    }
}
