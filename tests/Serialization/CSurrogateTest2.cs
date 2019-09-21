using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    /// <summary>
    /// This file tests all of the "setup" and data collection parts of the CSurrogate class.
    /// The other file CSurrogateTest tests the actual serialization/deserialization calls.
    ///</summary>
    [TestClass()]
    public class CSurrogateTest2
    {
        private const int EXPECTED_INDEX = 1;
        //BindingFlags ALL_BINDING_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSerialize()
        {
            var x = new CStdImplicitSurrogate();
            var s = new CSurrogate( typeof( CStdImplicitSurrogate ) );

            var doc = new XmlDocument();
            doc.LoadXml( "<_/>" );

            var isComplete = s.Serialize( x, doc.DocumentElement, null );

            Assert.AreEqual( true, isComplete, "Expected a complete serialization" );
            Assert.AreEqual( ETestStatus.IMPLICIT_SERIALIZER,
                             CStdImplicitSurrogate.STATUS,
                             "The status was not correctly set." );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestDeserialize()
        {
            CStdImplicitSurrogate x = null;
            var s = new CSurrogate( typeof( CStdImplicitSurrogate ) );

            var doc = new XmlDocument();
            doc.LoadXml( "<_ NAME=\"Mike\" AGE='69' />" );

            CStdBaseObject.STATUS = ETestStatus.NONE;
            var o = new CWorkingObject();
            var isComplete = s.Deserialize( o, doc.DocumentElement, null );
            x = (CStdImplicitSurrogate) o.WorkingObject;

            Assert.AreEqual( true, isComplete, "Expected the deserializer to be complete." );
            Assert.AreEqual( ETestStatus.IMPLICIT_DESERIALIZER,
                             CStdImplicitSurrogate.STATUS,
                             "The status was not correctly set." );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSerializeIncomplete()
        {
            var x = new CIncompleteImplicitSurrogate();
            var s = new CSurrogate( typeof( CIncompleteImplicitSurrogate ) );

            var doc = new XmlDocument();
            doc.LoadXml( "<_/>" );

            var isComplete = s.Serialize( x, doc.DocumentElement, null );

            Assert.AreEqual( false, isComplete, "Expected an incomplete serialization" );
            Assert.AreEqual( ETestStatus.IMPLICIT_SERIALIZER_INCOMPLETE,
                             CStdImplicitSurrogate.STATUS,
                             "The status was not correctly set." );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestDeserializeIncomplete()
        {
            CIncompleteImplicitSurrogate x = null;
            var s = new CSurrogate( typeof( CIncompleteImplicitSurrogate ) );

            var doc = new XmlDocument();
            doc.LoadXml( "<_ NAME=\"Mike\" AGE='69' Incomplete='Yes' />" );

            CStdBaseObject.STATUS = ETestStatus.NONE;
            var o = new CWorkingObject();
            var isComplete = s.Deserialize( o, doc.DocumentElement, null );
            x = (CIncompleteImplicitSurrogate) o.WorkingObject;

            Assert.AreEqual( false, isComplete, "Expected the deserializer to be incomplete." );
            Assert.AreEqual( ETestStatus.IMPLICIT_DESERIALIZER_INCOMPLETE,
                             CStdImplicitSurrogate.STATUS,
                             "The status was not correctly set." );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void TestDeserializeIncompleteWithError()
        {
            try
            {
                var s = new CSurrogate( typeof( CIncompleteImplicitSurrogate ) );

                var doc = new XmlDocument();
                doc.LoadXml( "<_ NAME=\"Mike\" AGE='69' />" );

                CStdBaseObject.STATUS = ETestStatus.NONE;
                var o = new CWorkingObject();

                var isComplete = s.Deserialize( o, doc.DocumentElement, null ); // Generate the error here.
            }
            catch (TargetInvocationException e)
            {
                throw e.GetBaseException();
            }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSerializeVoid()
        {
            var x = new CVoidImplicitSurrogate();
            var s = new CSurrogate( typeof( CVoidImplicitSurrogate ) );

            var doc = new XmlDocument();
            doc.LoadXml( "<_/>" );

            var isComplete = s.Serialize( x, doc.DocumentElement, null );

            Assert.AreEqual( true, isComplete, "Expected a complete serialization" );
            Assert.AreEqual( ETestStatus.IMPLICIT_SERIALIZER_VOID,
                             CStdImplicitSurrogate.STATUS,
                             "The status was not correctly set." );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestDeserializeVoid()
        {
            CVoidImplicitSurrogate x = null;
            var s = new CSurrogate( typeof( CVoidImplicitSurrogate ) );

            var doc = new XmlDocument();
            doc.LoadXml( "<_ NAME=\"Mike\" AGE='69' />" );

            CStdBaseObject.STATUS = ETestStatus.NONE;
            var o = new CWorkingObject();
            var isComplete = s.Deserialize( o, doc.DocumentElement, null );
            x = (CVoidImplicitSurrogate) o.WorkingObject;

            Assert.AreEqual( true, isComplete, "Expected the deserializer to be complete." );
            Assert.AreEqual( ETestStatus.IMPLICIT_DESERIALIZER_VOID,
                             CStdImplicitSurrogate.STATUS,
                             "The status was not correctly set." );
        }


        public class CNothingImplicitHere
        {
            public CNothingImplicitHere()
            {
            }

            public void Serialize( XmlNode _node )
            {
            }

            public static object MayBeFromXml( XmlNode _xml ) => null;
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestNoSurrogate()
        {
            var x = new CNothingImplicitHere();
            var s = new CSurrogate( typeof( CNothingImplicitHere ) );

            var isComplete = s.Serialize( x, null, null );
            Assert.AreEqual( false, isComplete, "Expected the surrogate NOT to complete serialization" );

            var newObj = new CWorkingObject();
            isComplete = s.Deserialize( newObj, null, null );

            Assert.AreEqual( false, isComplete, "Expected the surrogate NOT to compelte deserialization" );
            Assert.IsNull( newObj.WorkingObject, "Did not expect any object to be returned from Deserialization" );
        }
    }
}
