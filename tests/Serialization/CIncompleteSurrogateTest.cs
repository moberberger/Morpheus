using Microsoft.VisualStudio.TestTools.UnitTesting;
using Morpheus.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    [TestClass]
    public class CIncompleteSurrogateTest
    {
        public class CTypeCounter : IExternalSurrogate
        {
            public string Name = "Jena Marlies";
            public string Address = "10 Home Way";
            public string Sex = "Female";

            [ADoNotSerialize]
            public int Count = 0;

            public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer )
            {
                Count++;
                return false;
            }

            public bool Deserialize( CWorkingObject _workingObject,
                                     XmlElement _parentNode,
                                     CDeserializer _deserializer )
            {
                Count++;
                return false;
            }
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestStringCounter()
        {
            var x = new CTypeCounter();

            var c = new CSerializationContext();
            c.SetConcise();
            c.RegisterExternalSurrogate( typeof( string ), x );

            var s = new CSerializer( c );
            var doc = s.Serialize( x );
            Assert.AreEqual( 3, x.Count, "There should have been 3 strings counted." );

            Console.WriteLine( "Depth of resulting XML: " + XmlExtensions.Depth( doc ) );
            Console.WriteLine( "Length of resulting XML String: " + doc.OuterXml.Length );
            Console.WriteLine( "Number of resulting XmlElements: " + XmlExtensions.ElementCount( doc ) );
            Print( doc );

            var d = new CDeserializer( c );
            var y = d.Deserialize<CTypeCounter>( doc );

            Assert.AreEqual( 0, y.Count, "The new object should have no counted strings" );
            Assert.AreEqual( 6, x.Count, "The initial object should have strings counted for 2 actions" );
        }


        public class CIntArraySurrogate : IExternalSurrogate
        {
            public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer )
            {
                var array = (int[]) _object;
                XmlExtensions.AddAttribute( _parentNode, "MYLEN", array.Length );
                return false;
            }

            public bool Deserialize( CWorkingObject _workingObject,
                                     XmlElement _parentNode,
                                     CDeserializer _deserializer )
            {
                var sLen = XmlExtensions.GetAttributeValue( _parentNode, "MYLEN" );
                var len = int.Parse( sLen );
                _workingObject.Set( Array.CreateInstance( typeof( int ), len + 1 ) );
                // This actually is a no-no, but i'm testing to make sure
                // that this is really the array that's being returned.
                return false;
            }
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestArraySurrogate()
        {
            var x = new int[] { 1, 3, 5, 7, 9 };

            var c = new CSerializationContext();
            c.SetConcise();
            var helper = new CIntArraySurrogate();
            c.RegisterExternalSurrogate( typeof( int[] ), helper );

            var s = new CSerializer( c );
            var doc = s.Serialize( x );

            Console.WriteLine( "Depth of resulting XML: " + XmlExtensions.Depth( doc ) );
            Console.WriteLine( "Length of resulting XML String: " + doc.OuterXml.Length );
            Console.WriteLine( "Number of resulting XmlElements: " + XmlExtensions.ElementCount( doc ) );
            Print( doc );

            var d = new CDeserializer( c );
            var y = (int[]) d.Deserialize( doc );

            Assert.AreEqual( x.Length, y.Length - 1, "Length of resulting array is wrong" );
            for (var i = 0; i < x.Length; i++)
            {
                Assert.AreEqual( x[i], y[i], "Invalid element at: " + i );
            }
        }


        public class CFriendSerializer : IExternalSurrogate
        {
            public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _serializer )
            {
                var f = (CFriend) _object;
                _serializer.IgnoreField( "FriendPerson" );
                var idx = Array.IndexOf( CBigPerson.People, f.FriendPerson );
                XmlExtensions.AddElement( _parentNode, "FriendPerson", idx );
                return false;
            }

            private List<int> m_indicies = null;

            public bool Deserialize( CWorkingObject _workingObject, XmlElement _parentNode, CDeserializer _framework )
            {
                _framework.IgnoreField( "FriendPerson" );
                var sIdx = XmlExtensions.GetElementValue( _parentNode, "FriendPerson" );
                var idx = int.Parse( sIdx );

                if (m_indicies == null)
                    m_indicies = new List<int>();
                m_indicies.Add( idx );

                return false;
            }

            public void FinishDeserializing( CBigPerson[] _array )
            {
                var numPeople = _array.Length;
                var indexPtr = 0;

                for (var p = 0; p < numPeople; p++)
                {
                    var person = _array[p];
                    for (var f = 0; f < person.Friends.Length; f++)
                    {
                        var friend = person.Friends[f];
                        var friendIndex = m_indicies[indexPtr++];
                        friend.FriendPerson = _array[friendIndex];
                    }
                }

                m_indicies = null;
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestExternalSurrogate()
        {
            CBigPerson.GenerateData( 100 );
            var c = new CSerializationContext();
            c.SetConcise();
            var helper = new CFriendSerializer();
            c.RegisterExternalSurrogate( typeof( CFriend ), helper );

            var s = new CSerializer( c );
            var doc = s.Serialize( CBigPerson.People );

            Console.WriteLine( "Depth of resulting XML: " + XmlExtensions.Depth( doc ) );
            Console.WriteLine( "Length of resulting XML String: " + doc.OuterXml.Length );
            Console.WriteLine( "Number of resulting XmlElements: " + XmlExtensions.ElementCount( doc ) );
            Print( doc );

            var d = new CDeserializer( c );
            var x2 = d.Deserialize<CBigPerson[]>( doc );
            helper.FinishDeserializing( x2 );

            CDeserializeTest.AssertEqualBigPeopleArray( CBigPerson.People, x2 );
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

        public static void Print() => Console.WriteLine(
                "\r\n\r\n-----------------------------------------------------------------------------\r\n\r\n" );

        [TestInitialize]
        public void ClearContext() => CSerializationContext.Global.ResetToGlobalDefault();
    }
}
