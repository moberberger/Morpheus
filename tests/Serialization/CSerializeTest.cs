using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

#pragma warning disable 0169

namespace Morpheus.Standard.UnitTests.Serialization
{
    [TestClass]
    public partial class CSerializeTest
    {
        private class CFieldRenamer : IFieldRenamer
        {
            public string ConvertFieldName( string _fieldName, FieldInfo _fieldInfo )
            {
                var name = CFixM_.ConvertName( _fieldName );

                var prefix = (_fieldInfo.FieldType == typeof( int )) ? "INT_" : "STRING_";

                return prefix + name;
            }
        }


        [TestInitialize]
        public void ClearContext() => CSerializationContext.Global.ResetToGlobalDefault();


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestFieldRenamer()
        {
            var c = new CSerializationContext
            {
                FieldRenamer = new CFieldRenamer()
            };

            var s = new CSerializer( c );
            var add = new CAddress();

            var doc = s.Serialize( add );
            Print( doc );

            var root = doc.DocumentElement;

            TestSingleRenamedField( add.m_zip, root["INT_Zip"] );
            TestSingleRenamedField( add.m_city, root["STRING_City"] );
            TestSingleRenamedField( add.m_street, root["STRING_Street"] );


            var d = new CDeserializer( c );
            var ad2 = d.Deserialize<CAddress>( doc );

            Assert.AreEqual( add.m_city, ad2.m_city, "City" );
            Assert.AreEqual( add.m_street, ad2.m_street, "Street" );
            Assert.AreEqual( add.m_zip, ad2.m_zip, "Zip" );
        }

        private void TestSingleRenamedField( object _expected, XmlElement _actualElement )
        {
            var expected = _expected.ToString();

            if (_actualElement == null)
                Assert.Fail( "The element was not found for " + expected );

            Assert.AreEqual( expected, _actualElement.InnerText, "Field Not Correct" );
        }


        private class NoGoodStuff
        {
            private readonly DSomeDelegate m_delegate;
            private readonly IntPtr m_intPtr;
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestPointersEtc()
        {
            var x = new NoGoodStuff();

            var c = new CSerializationContext();
            c.SetVerbose();
            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            var elem = doc.DocumentElement;

            Print( doc );

            Assert.AreEqual( 0, elem.ChildNodes.Count, "Should have no fields serialized" );
            Assert.AreEqual( 1, elem.Attributes.Count, "Should have one (the _Type) attribute" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestInvalidType()
        {
            object x = new IntPtr();

            var s = new CSerializer();
            var doc = s.Serialize( x );
            var elem = doc.DocumentElement;

            Print( doc );

            Assert.AreEqual( 0, elem.ChildNodes.Count, "Should be no children" );
            Assert.AreEqual( 1, elem.Attributes.Count, "Should be only 1 attribute (the Type)" );
            Assert.AreEqual( x.GetType().AssemblyQualifiedName,
                             XmlExtensions.GetAttributeValue( elem, s.Context.TypeAttributeName ),
                             "The Type attribute is wrong" );
        }

        private class CLinkedList
        {
            public int Data;
            public CLinkedList Next;
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestCircularReference()
        {
            var a = new CLinkedList
            {
                Data = 69
            };

            var b = new CLinkedList
            {
                Data = 70
            };

            a.Next = b;
            b.Next = a;

            var c = new CSerializationContext();
            c.SetVerbose();
            var s = new CSerializer( c );
            var doc = s.Serialize( a );
            Print( doc );
            var elem = doc.DocumentElement;

            var id = XmlExtensions.GetAttributeValue( elem, c.ReferenceIdAttributeName );
            Assert.AreEqual( "69", XmlExtensions.GetElementValue( elem, "Data" ), "The first node's data is wrong" );
            var e2 = (XmlElement) elem.SelectSingleNode( "Next" );
            Assert.AreEqual( "70", XmlExtensions.GetElementValue( e2, "Data" ), "The second node's data is wrong" );

            var e3 = (XmlElement) e2.SelectSingleNode( "Next" );
            var rid = XmlExtensions.GetAttributeValue( e3, c.ReferToAttributeName );
            Assert.AreEqual( id, rid, "The reference to the first node is wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestDuplicateReferences()
        {
            var arr = new object[10];
            var p = new CPerson();
            var p2 = new CPerson();

            arr[1] = p;
            arr[2] = p2;
            arr[5] = p2;
            arr[8] = p;
            arr[9] = p;

            object x = arr;

            var c = new CSerializationContext();
            c.SetConcise();
            c.FixM_ = true;
            c.ArrayElementsIncludeIndicies = true;
            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            AssureCorrectReference( elem, 1, 8 );
            AssureCorrectReference( elem, 1, 9 );
            AssureCorrectReference( elem, 2, 5 );
        }

        private static void AssureCorrectReference( XmlElement elem, int a1, int a2 )
        {
            var n = elem.SelectSingleNode( "//_[@_I=" + a1 + "]" );
            var id = XmlExtensions.GetAttributeValue( n, "_ID" );

            var n2 = elem.SelectSingleNode( "//_[@_I=" + a2 + "]" );
            var rid = XmlExtensions.GetAttributeValue( n2, "_RID" );

            Assert.AreEqual( id, rid, "ID of referenced element didn't match" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestNullableType()
        {
            int? x = null;
            var s = new CSerializer();
            CSerializationContext.Global.SetVerbose();

            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;
            Assert.AreEqual( CSerializationContext.Global.NullAttributeValue,
                             XmlExtensions.GetAttributeValue( elem, CSerializationContext.Global.NullAttributeName ),
                             "Should be null" );
            Assert.AreEqual( "", elem.InnerText, "Should be no innerText" );

            PrintLine();

            x = 69;
            doc = s.Serialize( x );
            Print( doc );
            elem = doc.DocumentElement;
            Assert.AreEqual( x.GetType().AssemblyQualifiedName,
                             XmlExtensions.GetAttributeValue( elem, CSerializationContext.Global.TypeAttributeName ),
                             "The Type is wrong" );
        }


        [Flags]
        private enum ETest
        {
            FIRST = 0x01,
            SECOND = 0x02,
            THIRD = 0X04,
            FOURTH = 0X08
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestEnumFlags()
        {
            var x = ETest.SECOND | ETest.FOURTH;

            var s = new CSerializer();
            CSerializationContext.Global.SetVerbose();

            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( "SECOND, FOURTH", elem.InnerText, "Enum serialized to wrong string" );

            PrintLine();

            x = 0;
            doc = s.Serialize( x );
            Print( doc );
            elem = doc.DocumentElement;
            Assert.AreEqual( "0", elem.InnerText, "invalid Enum value for ZERO" );
        }

        private class CClassWithObject
        {
            public object Obj;
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestEnumeration()
        {
            var x = ETestStatus.IMPLICIT_DESERIALIZER_INCOMPLETE;

            var s = new CSerializer();
            CSerializationContext.Global.SetVerbose();

            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( x.ToString(), elem.InnerText, "Enum serialized to wrong string" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void EnumValueAsObjectTest()
        {
            var obj = new CClassWithObject() { Obj = EDispatchMode.Threadpool };
            var s = new CSerializer();
            var doc = s.Serialize( obj );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSparseMultidimArray()
        {
            object[,,] x;
            x = (object[,,]) Array.CreateInstance( typeof( object ), new int[] { 3, 4, 5 }, new int[] { 3, 2, 1 } );
            x[3, 5, 3] = Guid.NewGuid();
            x[4, 4, 1] = "hello";
            x[4, 4, 2] = "Good Bye";
            x[4, 4, 3] = "he" + "llo"; // interns to "hello", and shared with the "hello" at [4,4,1]
            x[5, 2, 5] = 678;

            var c = new CSerializationContext();
            c.SetConcise();

            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( 5, elem.ChildNodes.Count, "Number of elements is wrong" );
            Assert.AreEqual( "3,4,5",
                             XmlExtensions.GetAttributeValue( elem, c.ArrayAttributeName ),
                             "Array Lengths are wrong" );
            Assert.AreEqual( "3,2,1",
                             XmlExtensions.GetAttributeValue( elem, c.ArrayLowerBoundAttribute ),
                             "Array Lowerbounds are wrong" );

            Assert.AreEqual( "3,5,3",
                             XmlExtensions.GetAttributeValue( elem.ChildNodes[0], c.ArrayIndexAttributeName ),
                             "Index value is wrong" );
            Assert.AreEqual( "4,4,1",
                             XmlExtensions.GetAttributeValue( elem.ChildNodes[1], c.ArrayIndexAttributeName ),
                             "Index value is wrong" );
            Assert.AreEqual( "5,2,5",
                             XmlExtensions.GetAttributeValue( elem.ChildNodes[4], c.ArrayIndexAttributeName ),
                             "Index value is wrong" );
            Assert.IsNull( XmlExtensions.GetAttributeValue( elem.ChildNodes[2], c.ArrayIndexAttributeName ),
                           "Should be no index attribute for sequential element" );
            Assert.IsNull( XmlExtensions.GetAttributeValue( elem.ChildNodes[3], c.ArrayIndexAttributeName ),
                           "Should be no index attribute for sequential element" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestMultidimStringArray()
        {
            string[,] x;
            x = (string[,]) Array.CreateInstance( typeof( string ), new int[] { 2, 2 }, new int[] { 5, 9 } );

            x[5, 9] = "first";
            x[5, 10] = "second";
            x[6, 9] = "third";
            x[6, 10] = "fourth";

            var c = new CSerializationContext();
            c.SetConcise();

            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( 1, elem.ChildNodes.Count, "Should be one child (an XmlText node)" );
            Assert.AreEqual( "2,2",
                             XmlExtensions.GetAttributeValue( elem, c.ArrayAttributeName ),
                             "Array Lengths are wrong" );
            Assert.AreEqual( "5,9",
                             XmlExtensions.GetAttributeValue( elem, c.ArrayLowerBoundAttribute ),
                             "Array Lowerbounds are wrong" );
            Assert.AreEqual( "first,second,third,fourth", elem.InnerText, "The text for the multidim array is wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestMultidimArray()
        {
            var x = new int[3, 3, 5];
            for (var j = 0; j < 3; j++)
            {
                for (var k = 0; k < 3; k++)
                {
                    for (var l = 0; l < 5; l++)
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
            var elem = doc.DocumentElement;

            Assert.AreEqual( 3 * 3 * 5, elem.ChildNodes.Count, "Child node count is wrong" );
            Assert.AreEqual( "3,3,5",
                             XmlExtensions.GetAttributeValue( elem, c.ArrayAttributeName ),
                             "Array Lengths are wrong" );
            Assert.AreEqual( "0,0,0",
                             XmlExtensions.GetAttributeValue( elem, c.ArrayLowerBoundAttribute ),
                             "Array lower-bounds are wrong" );

            PrintLine();

            c.SetConcise();
            doc = s.Serialize( x );
            Print( doc );
            elem = doc.DocumentElement;

            Assert.AreEqual( 1, elem.ChildNodes.Count, "Child node count is wrong" );
            Assert.IsTrue( elem.InnerText.StartsWith( "0,1,2,3,4,10,11,12" ), "The inner text doesn't look right" );
        }


        private class CClassWithRenamedArrayElements
        {
            public const string MY_ELEM_NAME = "MyElemName";
            public int[] m_defaultNames = new int[] { 3, 6, 5, 9, 2, 4, 74 };

            [AElementName( MY_ELEM_NAME )]
            public int[] m_myNames = new int[] { 34, 62, 565, 69, 6969, 9435, 23, 234, 744 };
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestRenamedArrayElements()
        {
            const string DEFAULT_NAME = "default";
            var o = new CClassWithRenamedArrayElements();

            var c = new CSerializationContext
            {
                ArrayElementsIncludeIndicies = true,
                AllArraysHaveExplicitElements = true,
                ArrayElementName = DEFAULT_NAME
            };
            var s = new CSerializer( c );

            var doc = s.Serialize( o );
            Print( doc );

            var e = doc.DocumentElement["m_defaultNames"];
            Assert.IsNotNull( e, "Expected to find an element for default element names" );

            for (var i = 0; i < o.m_defaultNames.Length; i++)
            {
                var expected = o.m_defaultNames[i];
                var child = (XmlElement) e.ChildNodes[i];

                Assert.AreEqual( c.ArrayElementName, child.Name, "The name of the XML element is wrong on default elements" );
                Assert.AreEqual( expected, int.Parse( child.InnerText ), "The value found at index " + i + " was wrong on default elements." );
            }

            e = doc.DocumentElement["m_myNames"];
            Assert.IsNotNull( e, "Expected to find an element for 'my' element names" );

            for (var i = 0; i < o.m_myNames.Length; i++)
            {
                var expected = o.m_myNames[i];
                var child = (XmlElement) e.ChildNodes[i];

                Assert.AreEqual( CClassWithRenamedArrayElements.MY_ELEM_NAME, child.Name, "The name of the XML element is wrong on 'my' elements" );
                Assert.AreEqual( expected, int.Parse( child.InnerText ), "The value found at index " + i + " was wrong on 'my' elements." );
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestObjectArray()
        {
            var x = new object[] { null, 47, new CPerson(), new int[] { 3, 1, 4, 1, 5, 9 } };

            var s = new CSerializer();
            CSerializationContext.Global.SetVerbose();

            var doc = s.Serialize( x );
            Print( doc );

            PrintLine();

            CSerializationContext.Global.SetFullNames();
            doc = s.Serialize( x );
            Print( doc );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestExternalBaseSurrogate()
        {
            var x = new CMySuperStd
            {
                Name = "Alyssa",
                Age = 21,
                Sex = "Yes"
            };

            var c = new CSerializationContext();

            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( 3, elem.ChildNodes.Count, "Child Node Count is wrong" );
            Assert.AreEqual( x.Name, XmlExtensions.GetElementValue( elem, "Name" ), "Name" );
            Assert.AreEqual( x.Age.ToString(), XmlExtensions.GetElementValue( elem, "Age" ), "Age" );
            Assert.AreEqual( x.Sex, XmlExtensions.GetElementValue( elem, "Sex" ), "Sex" );

            PrintLine();

            c.RegisterExternalSurrogate( typeof( CStdBaseObject ), new CStdExternalSurrogate() );

            doc = s.Serialize( x );
            Print( doc );
            elem = doc.DocumentElement;

            Assert.AreEqual( 1, elem.ChildNodes.Count, "Child Node Count is wrong" );
            Assert.AreEqual( x.Name, XmlExtensions.GetAttributeValue( elem, "NAME" ), "Name" );
            Assert.AreEqual( x.Age.ToString(), XmlExtensions.GetAttributeValue( elem, "AGE" ), "Age" );
            Assert.AreEqual( x.Sex, XmlExtensions.GetElementValue( elem, "Sex" ), "Sex" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestExternalSurrogate()
        {
            var x = new CStdBaseObject
            {
                Name = "Alyssa",
                Age = 21
            };
            var c = new CSerializationContext();

            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( 2, elem.ChildNodes.Count, "Child Node Count is wrong" );
            Assert.AreEqual( x.Name, XmlExtensions.GetElementValue( elem, "Name" ), "Name" );
            Assert.AreEqual( x.Age.ToString(), XmlExtensions.GetElementValue( elem, "Age" ), "Age" );

            Console.WriteLine(
                "\r\n\r\n-----------------------------------------------------------------------------\r\n\r\n" );
            c.RegisterExternalSurrogate( typeof( CStdBaseObject ), new CStdExternalSurrogate() );

            doc = s.Serialize( x );
            Print( doc );
            elem = doc.DocumentElement;

            Assert.AreEqual( 0, elem.ChildNodes.Count, "Child Node Count is wrong" );
            Assert.AreEqual( x.Name, XmlExtensions.GetAttributeValue( elem, "NAME" ), "Name" );
            Assert.AreEqual( x.Age.ToString(), XmlExtensions.GetAttributeValue( elem, "AGE" ), "Age" );
        }


        public class CSuperArray
        {
            public Array Arr;
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSuperclassedArray()
        {
            var x = new CSuperArray
            {
                Arr = new int[] { 1, 3, 5 }
            };

            var s = new CSerializer();
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( typeof( CSuperArray ).AssemblyQualifiedName,
                             XmlExtensions.GetAttributeValue( elem, s.Context.TypeAttributeName ),
                             "Type of the root node is wrong" );

            var e = (XmlElement) elem.SelectSingleNode( "Arr" );
            Assert.IsNotNull( e, "Missing Arr element" );
            Assert.AreEqual( x.Arr.GetType().AssemblyQualifiedName,
                             XmlExtensions.GetAttributeValue( e, s.Context.TypeAttributeName ),
                             "Type Attribute Error" );
            Assert.AreEqual( x.Arr.Length.ToString(),
                             XmlExtensions.GetAttributeValue( e, s.Context.ArrayAttributeName ),
                             "Length of Array Attribute is wrong" );
            Assert.AreEqual( "1,3,5", elem.InnerText, "Inner Text for the array" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestCPerson()
        {
            var x = new CPerson();
            CSerializationContext.Global.FixM_ = true;

            var s = new CSerializer();
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( x.m_name, XmlExtensions.GetElementValue( elem, "Name" ), "Name" );
            Assert.AreEqual( x.m_age.ToString(), XmlExtensions.GetElementValue( elem, "Age" ), "Age" );

            var e = (XmlElement) elem.SelectSingleNode( "KidsNames" );
            Assert.IsNotNull( e, "Missing KidsNames element" );
            Assert.AreEqual( "3", XmlExtensions.GetAttributeValue( e, s.Context.ArrayAttributeName ), "Kids Array Count" );
            Assert.AreEqual( "Maggie,Lisa,Bart", e.InnerText, "Kids Names" );

            e = (XmlElement) elem.SelectSingleNode( "KidsAges" );
            Assert.IsNotNull( e, "Missing KidsNames element" );
            Assert.AreEqual( "3",
                             XmlExtensions.GetAttributeValue( e, s.Context.ArrayAttributeName ),
                             "KidsAges Array Count" );
            Assert.AreEqual( "1,7,9", e.InnerText, "Kids Names" );

            e = (XmlElement) elem.SelectSingleNode( "ANullValue" );
            Assert.IsNotNull( e, "Missing ANullValue element" );
            Assert.AreEqual( s.Context.NullAttributeValue,
                             XmlExtensions.GetAttributeValue( e, s.Context.NullAttributeName ),
                             "Null Attribute Error" );

            e = (XmlElement) elem.SelectSingleNode( "Address" );
            Assert.IsNotNull( e, "Missing Address element" );
            Assert.AreEqual( x.m_address.m_city, XmlExtensions.GetElementValue( e, "City" ), "Address-City" );
            Assert.AreEqual( x.m_address.m_street, XmlExtensions.GetElementValue( e, "Street" ), "Address-Street" );
            Assert.AreEqual( x.m_address.m_zip.ToString(), XmlExtensions.GetElementValue( e, "Zip" ), "Address-Zip" );

            e = (XmlElement) elem.SelectSingleNode( "OtherAddress" );
            Assert.IsNotNull( e, "Other Address Missing" );
            var sa = x.m_otherAddress as CSuperAddress;
            Assert.AreEqual( sa.m_country, XmlExtensions.GetElementValue( e, "Country" ), "OtherAddress-Country" );
            Assert.AreEqual( sa.m_city, XmlExtensions.GetElementValue( e, "City" ), "Address-City" );
            Assert.AreEqual( sa.m_street, XmlExtensions.GetElementValue( e, "Street" ), "Address-Street" );
            Assert.AreEqual( sa.m_zip.ToString(), XmlExtensions.GetElementValue( e, "Zip" ), "Address-Zip" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestCPersonExplicitArrays()
        {
            var x = new CPerson();
            var c = new CSerializationContext
            {
                FixM_ = true,
                AllArraysHaveExplicitElements = true,
                ArrayElementsIncludeIndicies = true
            };

            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( x.m_name, XmlExtensions.GetElementValue( elem, "Name" ), "Name" );
            Assert.AreEqual( x.m_age.ToString(), XmlExtensions.GetElementValue( elem, "Age" ), "Age" );

            var e = (XmlElement) elem.SelectSingleNode( "KidsNames" );
            Assert.IsNotNull( e, "Missing KidsNames element" );
            Assert.AreEqual( "3", XmlExtensions.GetAttributeValue( e, s.Context.ArrayAttributeName ), "Kids Array Count" );
            for (var i = 0; i < x.m_kidsNames.Length; i++)
            {
                Assert.AreEqual( x.m_kidsNames[i], e.ChildNodes[i].InnerText, "Kid " + i );
                Assert.AreEqual( i.ToString(),
                                 XmlExtensions.GetAttributeValue( e.ChildNodes[i], s.Context.ArrayIndexAttributeName ),
                                 "Array Index " + i );
            }

            e = (XmlElement) elem.SelectSingleNode( "ANullValue" );
            Assert.IsNotNull( e, "Missing ANullValue element" );
            Assert.AreEqual( s.Context.NullAttributeValue,
                             XmlExtensions.GetAttributeValue( e, s.Context.NullAttributeName ),
                             "Null Attribute Error" );

            e = (XmlElement) elem.SelectSingleNode( "Address" );
            Assert.IsNotNull( e, "Missing Address element" );
            Assert.AreEqual( x.m_address.m_city, XmlExtensions.GetElementValue( e, "City" ), "Address-City" );
            Assert.AreEqual( x.m_address.m_street, XmlExtensions.GetElementValue( e, "Street" ), "Address-Street" );
            Assert.AreEqual( x.m_address.m_zip.ToString(), XmlExtensions.GetElementValue( e, "Zip" ), "Address-Zip" );

            e = (XmlElement) elem.SelectSingleNode( "OtherAddress" );
            Assert.IsNotNull( e, "Other Address Missing" );
            var sa = x.m_otherAddress as CSuperAddress;
            Assert.AreEqual( sa.m_country, XmlExtensions.GetElementValue( e, "Country" ), "OtherAddress-Country" );
            Assert.AreEqual( sa.m_city, XmlExtensions.GetElementValue( e, "City" ), "Address-City" );
            Assert.AreEqual( sa.m_street, XmlExtensions.GetElementValue( e, "Street" ), "Address-Street" );
            Assert.AreEqual( sa.m_zip.ToString(), XmlExtensions.GetElementValue( e, "Zip" ), "Address-Zip" );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestImplicitSerializer()
        {
            var x = new CStdImplicitSurrogate
            {
                Name = "Homer",
                Age = 40
            };

            var s = new CSerializer();
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( 0, elem.ChildNodes.Count, "Node Count" );
            Assert.AreEqual( x.Name, XmlExtensions.GetAttributeValue( elem, "NAME" ), "Name" );
            Assert.AreEqual( x.Age.ToString(), XmlExtensions.GetAttributeValue( elem, "AGE" ), "Age" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSparseArray()
        {
            var x = new string[100];
            x[5] = "Something";
            x[49] = "Else";
            x[50] = "?";
            x[75] = "";

            var c = new CSerializationContext
            {
                AllArraysHaveExplicitElements = true,
                RemoveNullValuesFromXml = true
            };
            var s = new CSerializer( c );

            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( 4, elem.ChildNodes.Count, "Child node count is wrong" );
            Assert.AreEqual( x.Length.ToString(),
                             XmlExtensions.GetAttributeValue( elem, c.ArrayAttributeName ),
                             "Array Length value is wrong" );

            Assert.AreEqual( "5",
                             XmlExtensions.GetAttributeValue( elem.ChildNodes[0], c.ArrayIndexAttributeName ),
                             "Index for element 0 is wrong" );
            Assert.AreEqual( "49",
                             XmlExtensions.GetAttributeValue( elem.ChildNodes[1], c.ArrayIndexAttributeName ),
                             "Index for element 1 is wrong" );
            Assert.IsNull( XmlExtensions.GetAttributeValue( elem.ChildNodes[2], c.ArrayIndexAttributeName ),
                           "Should have no index attribute on consecutive elements" );
            Assert.AreEqual( "75",
                             XmlExtensions.GetAttributeValue( elem.ChildNodes[3], c.ArrayIndexAttributeName ),
                             "Index for element 3 is wrong" );

            Assert.AreEqual( x[5], elem.ChildNodes[0].InnerText, "Value of child 0 incorrect" );
            Assert.AreEqual( x[49], elem.ChildNodes[1].InnerText, "Value of child 1 incorrect" );
            Assert.AreEqual( x[50], elem.ChildNodes[2].InnerText, "Value of child 2 incorrect" );
            Assert.AreEqual( x[75], elem.ChildNodes[3].InnerText, "Value of child 3 incorrect" );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestNonstandardArray()
        {
            var x = Array.CreateInstance( typeof( int ), new int[] { 10 }, new int[] { 5 } );
            var s = new CSerializer();

            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( x.GetLowerBound( 0 ).ToString(),
                             XmlExtensions.GetAttributeValue( elem, s.Context.ArrayLowerBoundAttribute ),
                             "Lower Bound not set right" );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestStdBaseObject()
        {
            var x = new CStdBaseObject
            {
                Name = "Homer",
                Age = 40
            };

            var s = new CSerializer();
            var doc = s.Serialize( x );
            Print( doc );
            var elem = doc.DocumentElement;

            Assert.AreEqual( 2, elem.ChildNodes.Count, "Node Count" );
            Assert.AreEqual( x.Name, elem.SelectSingleNode( "Name" ).InnerText, "Name" );
            Assert.AreEqual( x.Age.ToString(), elem.SelectSingleNode( "Age" ).InnerText, "Age" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestArray_Primitive()
        {
            var a = new int[] { 1, 2, 3 };

            var s = new CSerializer();
            var doc = s.Serialize( a );
            Print( doc );
            var e = doc.DocumentElement;

            Assert.AreEqual( a.GetType().AssemblyQualifiedName,
                             XmlExtensions.GetAttributeValue( e, s.Context.TypeAttributeName ),
                             "The Type attribute is wrong" );
            Assert.AreEqual( int.Parse( XmlExtensions.GetAttributeValue( e, s.Context.ArrayAttributeName ) ),
                             a.Length,
                             "Array Attribute has wrong length value" );
            Assert.AreEqual( "1,2,3", e.InnerText, "Comma-Separated List is wrong." );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestArray_String()
        {
            var a = new string[] { "hello,", "homer", "", "!", null, "what?" };

            var context = new CSerializationContext();

            var s = new CSerializer( context );
            var doc = s.Serialize( a );
            Print( doc );
            var e = doc.DocumentElement;

            Assert.AreEqual( a.GetType().AssemblyQualifiedName,
                             XmlExtensions.GetAttributeValue( e, s.Context.TypeAttributeName ),
                             "The Type attribute is wrong" );
            Assert.AreEqual( int.Parse( XmlExtensions.GetAttributeValue( e, s.Context.ArrayAttributeName ) ),
                             a.Length,
                             "Array Attribute has wrong length value" );
            Assert.AreEqual( @"hello\`,homer,\_,!,,what?", e.InnerText, "Comma-Separated List is wrong." );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSerializeString()
        {
            var x = "Homer";

            var s = new CSerializer();
            var doc = s.Serialize( x );
            Print( doc );
            var e = doc.DocumentElement;

            Assert.AreEqual( s.Context.RootElementName, e.Name, "Root name is incorrect" );
            Assert.AreEqual( x.GetType().AssemblyQualifiedName,
                             XmlExtensions.GetAttributeValue( e, s.Context.TypeAttributeName ),
                             "Type attribute is incorrect" );
            Assert.AreEqual( x, e.InnerText, "The value itself was wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSerializePrimitive()
        {
            var x = 69;

            var s = new CSerializer();
            var doc = s.Serialize( x );
            Print( doc );
            var e = doc.DocumentElement;

            Assert.AreEqual( s.Context.RootElementName, e.Name, "Root name is incorrect" );
            Assert.AreEqual( x.GetType().AssemblyQualifiedName,
                             XmlExtensions.GetAttributeValue( e, s.Context.TypeAttributeName ),
                             "Type attribute is incorrect" );
            Assert.AreEqual( x.ToString(), e.InnerText, "The value itself was wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestNullSerialize()
        {
            var s = new CSerializer();
            var doc = s.Serialize( null );
            Print( doc );
            var e = doc.DocumentElement;

            Assert.AreEqual( s.Context.RootElementName, e.Name, "Root name is incorrect" );
            Assert.AreEqual( s.Context.NullAttributeValue,
                             XmlExtensions.GetAttributeValue( e, s.Context.NullAttributeName ),
                             "Null attribute was not there" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestConstructor()
        {
            var other = new CSerializationContext();
            var s = new CSerializer( other );

            ReferenceEquals( other, s.Context );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestConstructorNull()
        {
            var s = new CSerializer( null );
            Assert.AreEqual( CSerializationContext.Global,
                             s.Context,
                             "Null constructor parameter should point to Global context" );
        }


        public class CClassWithType
        {
            public Type Type;
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestTypeFieldTest()
        {
            var obj = new CClassWithType() { Type = GetType() };
            var s = new CSerializer();
            var doc = s.Serialize( obj );
        }




        public static void Print( XmlDocument _doc )
        {
            var s = new StringWriter();
            var xs = new XmlTextWriter( s )
            {
                Formatting = Formatting.Indented,
                Indentation = 8
            };

            _doc.Save( xs );
            Console.WriteLine( s );
        }

        public static void PrintLine() => Console.WriteLine(
                "\r\n\r\n-----------------------------------------------------------------------------\r\n\r\n" );


    }
}