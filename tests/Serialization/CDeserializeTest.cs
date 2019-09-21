using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    [TestClass]
    public class CDeserializeTest : CBaseXmlPrinter
    {
        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestNullableValue()
        {
            int? x, y;
            var s = new CSerializer();
            var d = new CDeserializer();
            XmlDocument doc;

            x = 5;
            doc = s.Serialize( x );
            Print( doc );
            y = d.Deserialize<int?>( doc );
            Console.WriteLine( "Deserialized: {0}", (y == null) ? "<null>" : y.ToString() );

            x = null;
            doc = s.Serialize( x );
            Print( doc );
            y = d.Deserialize<int?>( doc );
            Console.WriteLine( "Deserialized: {0}", (y == null) ? "<null>" : y.ToString() );
        }

        [ExpectedException( typeof( NullReferenceException ) )]
        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestNullableValueError()
        {
            int? x = null;

            var s = new CSerializer();
            var doc = s.Serialize( x );

            Print( doc );

            var d = new CDeserializer();
            var y = (int) d.Deserialize( doc ); // should throw the error
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestArraysNullAndEmpty()
        {
            var x = new int[0];

            var c = new CSerializationContext();
            c.SetVerbose();
            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Print( doc );
            var d = new CDeserializer( c );
            var y = (int[]) d.Deserialize( doc );
            Assert.AreEqual( 0, y.Length, "Resulting array length should be zero" );

            Print();
            c.SetConcise();
            doc = s.Serialize( x );
            Print( doc );
            y = d.Deserialize<int[]>( doc );
            Assert.AreEqual( 0, y.Length, "Resulting array length should be zero" );

            Print();
            var sx = new string[0];
            c.SetVerbose();
            doc = s.Serialize( sx );
            Print( doc );
            var sy = (string[]) d.Deserialize( doc );
            Assert.AreEqual( 0, sy.Length, "Resulting array length should be zero" );

            Print();
            c.SetConcise();
            doc = s.Serialize( sx );
            Print( doc );
            sy = d.Deserialize<string[]>( doc );
            Assert.AreEqual( 0, sy.Length, "Resulting array length should be zero" );

            Print();
            sx = new string[1];
            sx[0] = null;
            doc = s.Serialize( sx );
            Print( doc );
            sy = d.Deserialize<string[]>( doc );
            Assert.AreEqual( 1, sy.Length, "Resulting array length should be zero" );
            Assert.IsNull( sy[0], "First element should be null" );

            Print();
            c.SetVerbose();
            doc = s.Serialize( sx );
            Print( doc );
            sy = (string[]) d.Deserialize( doc );
            Assert.AreEqual( 1, sy.Length, "Resulting array length should be zero" );
            Assert.IsNull( sy[0], "First element should be null" );

            Print();
            c.SetConcise();
            sx = new string[2];
            sx[0] = null;
            sx[1] = "";
            doc = s.Serialize( sx );
            Print( doc );
            sy = d.Deserialize<string[]>( doc );
            Assert.AreEqual( 2, sy.Length, "Resulting array length should be zero" );
            Assert.IsNull( sy[0], "First element should be null" );
            Assert.AreEqual( string.Empty, sy[1], "Second element should be Empty" );

            Print();
            c.SetVerbose();
            doc = s.Serialize( sx );
            Print( doc );
            sy = (string[]) d.Deserialize( doc );
            Assert.AreEqual( 2, sy.Length, "Resulting array length should be zero" );
            Assert.IsNull( sy[0], "First element should be null" );
            Assert.AreEqual( string.Empty, sy[1], "Second element should be Empty" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void Test3DimArray()
        {
            var x = new int[3, 3, 5];
            for (var j = 0; j < x.GetLength( 0 ); j++)
            {
                for (var k = 0; k < x.GetLength( 1 ); k++)
                {
                    for (var l = 0; l < x.GetLength( 2 ); l++)
                    {
                        x[j, k, l] = j * 100 + k * 10 + l;
                    }
                }
            }

            var c = new CSerializationContext();
            c.SetVerbose();
            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Print( doc );
            var d = new CDeserializer( c );
            var y = (int[,,]) d.Deserialize( doc );

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual( x.GetLength( i ), y.GetLength( i ), "Array Length is wrong at: " + i );
            }

            for (var j = 0; j < x.GetLength( 0 ); j++)
            {
                for (var k = 0; k < x.GetLength( 1 ); k++)
                {
                    for (var l = 0; l < x.GetLength( 2 ); l++)
                    {
                        Assert.AreEqual( x[j, k, l], y[j, k, l], "Element is wrong: " + j + "," + k + "," + l );
                    }
                }
            }

            y = null;
            GC.Collect();

            c.SetConcise();
            doc = s.Serialize( x );
            Print( doc );
            y = d.Deserialize<int[,,]>( doc );

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual( x.GetLength( i ), y.GetLength( i ), "Array Length is wrong at: " + i );
            }

            for (var j = 0; j < x.GetLength( 0 ); j++)
            {
                for (var k = 0; k < x.GetLength( 1 ); k++)
                {
                    for (var l = 0; l < x.GetLength( 2 ); l++)
                    {
                        Assert.AreEqual( x[j, k, l], y[j, k, l], "Element is wrong: " + j + "," + k + "," + l );
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestBigPerson()
        {
            var src = new CBigPerson()
            {
                Role = CBigPerson.ERole.Parent
            };

            var ser = new CSerializer();

            var doc = ser.Serialize( src );

            var deser = new CDeserializer();
            var dest = deser.Deserialize<CBigPerson>( doc );

            Assert.AreEqual( src.Name, dest.Name, "Name" );
            Assert.AreEqual( src.Height, dest.Height, "Height" );
            Assert.AreEqual( src.IsParent, dest.IsParent, "IsParent" );
            Assert.AreEqual( src.Role, dest.Role, "Role" );
        }




        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestPeople()
        {
            CBigPerson.GenerateData( 100 );
            var c = new CSerializationContext();
            c.SetConcise();
            var s = new CSerializer( c );
            var doc = s.Serialize( CBigPerson.People );

            Console.WriteLine( "Depth of resulting XML: " + CXmlHelper.Depth( doc ) );
            Console.WriteLine( "Length of resulting XML String: " + doc.OuterXml.Length );
            Console.WriteLine( "Number of resulting XmlElements: " + CXmlHelper.ElementCount( doc ) );

            var d = new CDeserializer( c );

            var x2 = d.Deserialize<CBigPerson[]>( doc );
            var x1 = CBigPerson.People;

            AssertEqualBigPeopleArray( x1, x2 );
        }

        public static void AssertEqualBigPeopleArray( CBigPerson[] _first, CBigPerson[] _second )
        {
            Assert.AreEqual( _first.Length, _second.Length, "People Array Length Error" );
            for (var i = 0; i < _first.Length; i++)
            {
                Assert.IsFalse( ReferenceEquals( _first[i], _second[i] ) );

                Assert.AreEqual( _first[i].Name, _second[i].Name, "Name" );
                Assert.AreEqual( _first[i].Age, _second[i].Age, "Age" );
                Assert.AreEqual( _first[i].Height, _second[i].Height, 1e-6, "Height" );
                Assert.AreEqual( _first[i].IsParent, _second[i].IsParent, "IsParent" );

                Assert.AreEqual( _first[i].KidsNames.Length, _second[i].KidsNames.Length, "Kids Names Len" );
                Assert.AreEqual( _first[i].KidsAges.Length, _second[i].KidsAges.Length, "Kids Ages Len" );

                Assert.AreEqual( _first[i].Numbers.Length, _second[i].Numbers.Length, "Numbers Length" );
                Assert.AreEqual( _first[i].Friends.Length, _second[i].Friends.Length, "Friends Length" );

                for (var j = 0; j < _first[i].KidsNames.Length; j++)
                {
                    Assert.AreEqual( _first[i].KidsNames[j], _second[i].KidsNames[j], "Kids Name " + j );
                    Assert.AreEqual( _first[i].KidsAges[j], _second[i].KidsAges[j], "Kids Age " + j );
                }

                for (var j = 0; j < _first[i].Numbers.Length; j++)
                {
                    Assert.AreEqual( _first[i].Numbers[j], _second[i].Numbers[j], "Number at " + j );
                }

                for (var j = 0; j < _first[i].Friends.Length; j++)
                {
                    Assert.IsFalse( ReferenceEquals( _first[i].Friends[j], _second[i].Friends[j] ) );

                    Assert.AreEqual( _first[i].Friends[j].IsBestFriend,
                                     _second[i].Friends[j].IsBestFriend,
                                     "Is Best Friend " + j );
                    Assert.AreEqual( _first[i].Friends[j].Rating, _second[i].Friends[j].Rating, "Is Best Friend " + j );
                    Assert.AreEqual( _first[i].Friends[j].FriendPerson.Name,
                                     _second[i].Friends[j].FriendPerson.Name,
                                     "The Friend " + j );
                }
            }
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestCPersonArray()
        {
            var x = new CPerson[2];
            x[0] = new CPerson();
            x[1] = new CPerson();
            x[1].Alternate();

            var s = new CSerializer();
            var doc = s.Serialize( x );
            Print( doc );

            var d = new CDeserializer();
            var z = d.Deserialize<CPerson[]>( doc );

            Assert.AreEqual( 2, z.Length, "Array length is wrong" );
            Assert.AreEqual( x[0].m_name, z[0].m_name, "Name" );
            Assert.AreEqual( x[0].m_age, z[0].m_age, "Age" );
            Assert.AreEqual( x[0].m_address.m_city, z[0].m_address.m_city, "City" );
            Assert.AreEqual( x[0].m_address.m_street, z[0].m_address.m_street, "Street" );
            Assert.AreEqual( x[0].m_address.m_zip, z[0].m_address.m_zip, "Zip" );
            Assert.AreEqual( x[0].m_kidsNames[0], z[0].m_kidsNames[0], "Kids0" );
            Assert.AreEqual( x[0].m_kidsNames[1], z[0].m_kidsNames[1], "Kids1" );
            Assert.AreEqual( x[0].m_kidsNames[2], z[0].m_kidsNames[2], "Kids2" );
            Assert.AreEqual( x[0].m_kidsAges[0], z[0].m_kidsAges[0], "Kids0" );
            Assert.AreEqual( x[0].m_kidsAges[1], z[0].m_kidsAges[1], "Kids1" );
            Assert.AreEqual( x[0].m_kidsAges[2], z[0].m_kidsAges[2], "Kids2" );

            Assert.AreEqual( x[1].m_name, z[1].m_name, "1Name" );
            Assert.AreEqual( x[1].m_age, z[1].m_age, "1Age" );
            Assert.AreEqual( x[1].m_address.m_city, z[1].m_address.m_city, "1City" );
            Assert.AreEqual( x[1].m_address.m_street, z[1].m_address.m_street, "1Street" );
            Assert.AreEqual( x[1].m_address.m_zip, z[1].m_address.m_zip, "1Zip" );
            Assert.AreEqual( x[1].m_kidsNames[0], z[1].m_kidsNames[0], "1Kids0" );
            Assert.AreEqual( x[1].m_kidsNames[1], z[1].m_kidsNames[1], "1Kids1" );
            Assert.AreEqual( x[1].m_kidsNames[2], z[1].m_kidsNames[2], "1Kids2" );
            Assert.AreEqual( x[1].m_kidsAges[0], z[1].m_kidsAges[0], "1Kids0" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        [
            ExpectedException( typeof( XDeserializationError ),
                "A condensed array was deserialized into a non-primitive, non-string Type. It should have failed." )]
        public void TestCondensedArrayWithNonPrimitive()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<root _T='Morpheus.Standard.UnitTests.Serialization.CPerson[]'>1,2,3,4,5</root>" );

            var x = (CPerson[]) s.Deserialize( doc );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestPrimitiveArray()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml(
                "<root _T='System.Int32[]'> " +
                "    <_ _I='1'>11</_>       " +
                "    <_       >22</_>       " +
                "    <_ _I='4'>44</_>       " +
                "</root>                    " );

            var x = (int[]) s.Deserialize( doc );

            Assert.AreEqual( 5, x.Length, "Length of resulting array is wrong" );
            Assert.AreEqual( 0, x[0], "0" );
            Assert.AreEqual( 11, x[1], "1" );
            Assert.AreEqual( 22, x[2], "2" );
            Assert.AreEqual( 0, x[3], "3" );
            Assert.AreEqual( 44, x[4], "4" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestCondensedStringArray()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( @"<root _T='System.String[]'>Hello,,to,\_,all</root>" );

            var x = (string[]) s.Deserialize( doc );

            Assert.AreEqual( 5, x.Length, "Length of resulting array is wrong" );
            Assert.AreEqual( "Hello", x[0], "0" );
            Assert.AreEqual( null, x[1], "1" );
            Assert.AreEqual( "to", x[2], "2" );
            Assert.AreEqual( "", x[3], "3" );
            Assert.AreEqual( "all", x[4], "4" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestCondensedPrimitiveArray()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<root _T='System.Int32[]'>1,2,3,4,5</root>" );

            var x = (int[]) s.Deserialize( doc );

            Assert.AreEqual( 5, x.Length, "Length of resulting array is wrong" );
            for (var i = 0; i < x.Length; i++)
            {
                Assert.AreEqual( i + 1, x[i], "Element " + i + " is wrong" );
            }
        }


        private class CLinkedList
        {
            public int Data = 0;
            public CLinkedList Next = null;
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestCircularRef()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml(
                "<root _ID='1'>             " +
                "    <Data>44</Data>        " +
                "    <Next>                 " +
                "        <Data>55</Data>    " +
                "        <Next _RID='1'/>   " +
                "    </Next>                " +
                "</root>                    " );

            var x = s.Deserialize<CLinkedList>( doc );
            Assert.AreEqual( 44, x.Data, "Head node Data is wrong" );
            Assert.AreEqual( 55, x.Next.Data, "Head->Next node Data is wrong" );
            ReferenceEquals( x, x.Next.Next );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestImplicitSurrogate()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<root _T='Morpheus.Standard.UnitTests.Serialization.CStdImplicitSurrogate' NAME='Michael' AGE='22'/>" );

            var x = s.Deserialize<CStdBaseObject>( doc );
            Assert.AreEqual( "Michael", x.Name, "Name is wrong" );
            Assert.AreEqual( 22, x.Age, "Age is wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestExternalSurrogate()
        {
            var c = new CSerializationContext();
            c.RegisterExternalSurrogate( typeof( CStdBaseObject ), new CStdExternalSurrogate() );

            var s = new CDeserializer( c );
            var doc = new XmlDocument();
            doc.LoadXml( "<root NAME='Mike' AGE='11'/>" );

            var x = s.Deserialize<CStdBaseObject>( doc );
            Assert.AreEqual( "Mike", x.Name, "Name is wrong" );
            Assert.AreEqual( 11, x.Age, "Age is wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestStandardDeserialization()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml(
                "<Babe>                     " +
                "    <Name>Alyssa</Name>    " +
                "    <Age>24</Age>          " +
                "    <Sex>Lots</Sex>        " +
                "</Babe>                    " );

            var x = s.Deserialize<CMySuperStd>( doc );
            Assert.AreEqual( "Alyssa", x.Name, "Name is wrong" );
            Assert.AreEqual( 24, x.Age, "Age is wrong" );
            Assert.AreEqual( "Lots", x.Sex, "Sex is wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestStandardDeserializationBaseSurrogate()
        {
            var c = new CSerializationContext();
            c.RegisterExternalSurrogate( typeof( CStdBaseObject ), new CStdExternalSurrogate() );
            var s = new CDeserializer( c );
            var doc = new XmlDocument();
            doc.LoadXml(
                "<Babe NAME='Alyssa' AGE='24'>  " +
                "    <Sex>Lots</Sex>            " +
                "</Babe>                        " );

            var x = s.Deserialize<CMySuperStd>( doc );
            Assert.AreEqual( "Alyssa", x.Name, "Name is wrong" );
            Assert.AreEqual( 24, x.Age, "Age is wrong" );
            Assert.AreEqual( "Lots", x.Sex, "Sex is wrong" );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestInvalidTypeDeserialization()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<root _T='System.IntPtr'>0</root>" );

            var x = s.Deserialize( doc );
            Assert.IsNull( x, "Invalid type should deserialize to null" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestGenericDeserialize()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<root>45</root>" );

            var x = s.Deserialize<int>( doc );
            Assert.AreEqual( 45, x, "Deserialization failed for GenericFunction call" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        [
            ExpectedException( typeof( XDeserializationError ),
                "Expected an exception when the deserializer doesn't know what to deserialize" )]
        public void TestDeserializeNoType()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<root>45</root>" );

            s.Deserialize( doc );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestDeserializeNullAttribute()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<root _T='System.String' _N='1'>45</root>" );

            var o = s.Deserialize( doc );

            Assert.IsNull( o, "The returned object should be null!" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestDeserializeString()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<root _T='System.String'>45</root>" );

            var o = s.Deserialize( doc );

            Assert.AreEqual( typeof( string ), o.GetType(), "The type of the object returned is wrong" );
            Assert.AreEqual( "45", o.ToString(), "The value of the object is wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestDeserializePrimitive()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<root _T='System.Int32'>45</root>" );

            var o = s.Deserialize( doc );

            Assert.AreEqual( typeof( int ), o.GetType(), "The type of the object returned is wrong" );
            Assert.AreEqual( 45, (int) o, "The value of the object is wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestNullDeserialize()
        {
            var s = new CDeserializer();
            var doc = new XmlDocument();
            var o = s.Deserialize( doc );

            Assert.IsNull( o, "An XmlDocument with nothing in it should return nothing" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestConstructor()
        {
            var other = new CSerializationContext();
            var s = new CDeserializer( other );

            ReferenceEquals( other, s.Context );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestConstructorNull()
        {
            var s = new CDeserializer( null );
            Assert.AreEqual( CSerializationContext.Global,
                             s.Context,
                             "Null constructor parameter should point to Global context" );
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestInitialize]
        public void ClearContext()
        {
            CSerializationContext.Global.ResetToGlobalDefault();
            CSerializationContext.Global.SetConcise();
        }
    }
}