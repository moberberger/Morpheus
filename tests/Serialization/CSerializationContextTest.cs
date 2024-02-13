using Microsoft.VisualStudio.TestTools.UnitTesting;
using Morpheus.Serialization;
using System;
using System.Reflection;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    /// <summary>
    /// This is a test class for Oberberger.Morpheus.CSerializationContext and is intended
    /// to contain all Oberberger.Morpheus.CSerializationContext Unit Tests
    ///</summary>
    [TestClass()]
    public class CSerializationContextTest
    {
        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestUtcStringProperty()
        {
            // The default global property is FALSE for UTC strings.
            Assert.IsFalse( CSerializationContext.Global.UseFullUtcDateTimeStrings,
                            "The default global value of UtcStrings is FALSE." );

            var c = new CSerializationContext();
            Assert.IsFalse( c.UseFullUtcDateTimeStrings,
                            "Inheriting from the default global context should not use UtcStrings" );

            CSerializationContext.Global.UseFullUtcDateTimeStrings = true;
            Assert.IsTrue( CSerializationContext.Global.UseFullUtcDateTimeStrings,
                           "The After setting the global context to Use Utc Strings, the prop should be TRUE." );
            Assert.IsTrue( c.UseFullUtcDateTimeStrings,
                           "Because the global context changed, the dependant context should also change." );

            c.UseFullUtcDateTimeStrings = false;
            Assert.IsFalse( c.UseFullUtcDateTimeStrings,
                            "Setting the dependant context to FALSE should be reflected in the prop-Getter" );
            Assert.IsTrue( CSerializationContext.Global.UseFullUtcDateTimeStrings,
                           "Changing the Dependant context should not modify the Global context." );

            c.UseFullUtcDateTimeStrings = true; // This explicitly means that the dependant context should use strings.
            Assert.IsTrue( c.UseFullUtcDateTimeStrings,
                           "Setting the dependant context to TRUE should be reflected in the prop-Getter" );

            CSerializationContext.Global.UseFullUtcDateTimeStrings = false;
            Assert.IsFalse( CSerializationContext.Global.UseFullUtcDateTimeStrings,
                            "Make sure that the GLOBAL property is set to FALSE" );
            Assert.IsTrue( c.UseFullUtcDateTimeStrings,
                           "Make sure that the dependant context's override survives the modification of the Global value" );

            var d = new DateTime( 2000, 7, 5, 1, 2, 3 );
            var expected = d.ToString( CUtcDateTimeSurrogate.UTC_COMPLETE_DATE_TIME_FORMAT );

            var sur = new CUtcDateTimeSurrogate();
            var doc = new XmlDocument();
            doc.LoadXml( "<root />" );

            var retval = sur.Serialize( d, d.GetType(), doc.DocumentElement, null );
            Assert.AreEqual( expected,
                             doc.DocumentElement.InnerText,
                             "The UTC serialization didn't yield the correct string." );
            Assert.IsTrue( retval, "The surrogate should always indicate that it completely processed the object" );

            var w = new CWorkingObject();
            retval = sur.Deserialize( w, doc.DocumentElement, null );
            Assert.AreEqual( d, w.WorkingObject, "The deserialized DateTime is incorrect." );
            Assert.IsTrue( retval, "The surrogate should always indicate that it completely processed the object" );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestBasicOperation()
        {
            // Default construction inherits from Global
            var c = new CSerializationContext();
            c.SetShortNames();

            CSerializationContext.Global.FixM_ = true;
            Assert.AreEqual( true, c.FixM_, "Make sure instance inherits Global" );

            c.FixM_ = false;
            Assert.AreEqual( false, c.FixM_, "Make sure stuff set on the instance overrides global" );
            Assert.AreEqual( true, CSerializationContext.Global.FixM_, "The Global value should still be true" );

            Assert.AreEqual( false,
                             CSerializationContext.Global.RemoveNullValuesFromXml,
                             "The default for all properties is false" );
            Assert.AreEqual( false, c.RemoveNullValuesFromXml, "The default for all properties is false" );

            CSerializationContext.Global.RemoveNullValuesFromXml = true;

            Assert.AreEqual( true, CSerializationContext.Global.RemoveNullValuesFromXml, "Setting Global value" );
            Assert.AreEqual( true, c.RemoveNullValuesFromXml, "Settting global value should bubble-up" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestSpecificParent_Constructor()
        {
            var c1 = new CSerializationContext
            {
                ArrayElementsIncludeIndicies = true
            };

            Assert.AreEqual( false,
                             CSerializationContext.Global.ArrayElementsIncludeIndicies,
                             "The Global should still be false" );
            Assert.AreEqual( true, c1.ArrayElementsIncludeIndicies, "The instance should be TRUE" );

            var c2 = new CSerializationContext( c1 );

            Assert.AreEqual( true,
                             c2.ArrayElementsIncludeIndicies,
                             "Even though the instance default is false, the inherited context should be TRUE like its parent." );
            Assert.AreEqual( false,
                             CSerializationContext.Global.ArrayElementsIncludeIndicies,
                             "The Global should still be false" );

            c2.ArrayElementsIncludeIndicies = false;
            Assert.AreEqual( false, c2.ArrayElementsIncludeIndicies, "The instance should override the parent" );
            Assert.AreEqual( true, c1.ArrayElementsIncludeIndicies, "The parent should still be true" );
            Assert.AreEqual( false,
                             CSerializationContext.Global.ArrayElementsIncludeIndicies,
                             "The Global should still be false" );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestClearingFlags()
        {
            var c1 = new CSerializationContext
            {
                FixM_ = true,
                RemoveNullValuesFromXml = true
            };

            var c2 = new CSerializationContext( c1 )
            {
                FixM_ = false
            };

            Assert.AreEqual( false, CSerializationContext.Global.FixM_, "Global FixM" );
            Assert.AreEqual( false, CSerializationContext.Global.RemoveNullValuesFromXml, "Global Remove Nulls" );

            Assert.AreEqual( true, c1.FixM_, "C1 FixM" );
            Assert.AreEqual( true, c1.RemoveNullValuesFromXml, "C1 Remove Nulls" );

            Assert.AreEqual( false, c2.FixM_, "C2 FixM" );
            Assert.AreEqual( true, c2.RemoveNullValuesFromXml, "C2 Remove Nulls" );

            c2.ClearFlag( CSerializationContext.EContextFields.FIX_M_ );
            Assert.AreEqual( true, c2.FixM_, "PassThrough of Global's FixM" );

            CSerializationContext.Global.FixM_ = true;
            CSerializationContext.Global.ClearFlag( CSerializationContext.EContextFields.FIX_M_ );
            Assert.AreEqual( true, CSerializationContext.Global.FixM_, "Global FixM after clear" );
        }


        private class CMyFieldRenamer : IFieldRenamer
        {
            public string ConvertFieldName( string m_fieldName, FieldInfo _fieldInfo ) => throw new Exception( "The method or operation is not implemented." );
        }


        private class CMyExternalSurrogate : IExternalSurrogate
        {
            #region IExternalSurrogate Members
            public bool Serialize( object _object, Type _useType, XmlElement _parentNode, CSerializer _framework ) => throw new Exception( "The method or operation is not implemented." );

            public bool Deserialize( CWorkingObject _object, XmlElement _parentNode, CDeserializer _framework ) => throw new Exception( "The method or operation is not implemented." );
            #endregion
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestExternalSurrogates()
        {
            var c = new CSerializationContext();

            var type1 = typeof( CSerializationContextTest );
            // its unimportant WHAT type to use- the surrogate is never called anyway
            var type2 = typeof( CMyFieldRenamer );

            Assert.IsNull( CSerializationContext.Global.GetExternalSurrogate( type1 ) );
            Assert.IsNull( c.GetExternalSurrogate( type1 ) );

            IExternalSurrogate surrogate = new CMyExternalSurrogate();

            CSerializationContext.Global.RegisterExternalSurrogate( type1, surrogate );

            Assert.AreEqual( surrogate, CSerializationContext.Global.GetExternalSurrogate( type1 ), "message" );
            Assert.AreEqual( surrogate, c.GetExternalSurrogate( type1 ), "message" );

            c.RegisterExternalSurrogate( type2, surrogate );
            Assert.IsNull( CSerializationContext.Global.GetExternalSurrogate( type2 ) );
            Assert.AreEqual( surrogate, c.GetExternalSurrogate( type2 ), "message" );


            CSerializationContext.Global.RemoveExternalSurrogate( type1 );
            Assert.IsNull( CSerializationContext.Global.GetExternalSurrogate( type1 ) );
            Assert.IsNull( c.GetExternalSurrogate( type1 ) );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestExplicitContext()
        {
            CSerializationContext.Global.AllArraysHaveExplicitElements = true; // change the "GLobal" value

            var c = new CSerializationContext
            {
                AllArraysHaveExplicitElements = false
            };

            Assert.AreEqual( true,
                             CSerializationContext.Global.AllArraysHaveExplicitElements,
                             "global should be true still" );
            Assert.AreEqual( false, c.AllArraysHaveExplicitElements, "Instance should be false" );

            var c2 = new CSerializationContext();
            Assert.AreEqual( true, c2.AllArraysHaveExplicitElements, "c2 should inherit Global's TRUE" );

            Assert.AreEqual( true, c2.GetFlag( CSerializationContext.EContextFields.ARRAYS_WITH_EXPLICIT_ELEMENTS ), "GetFlag(default) should use global" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestNullExternalTypes()
        {
            try
            {
                CSerializationContext.Global.RegisterExternalSurrogate( null, null );
            }
            catch (ArgumentNullException)
            {
                try
                {
                    CSerializationContext.Global.RemoveExternalSurrogate( null );
                }
                catch (ArgumentNullException)
                {
                    try
                    {
                        CSerializationContext.Global.GetExternalSurrogate( null );
                    }
                    catch (ArgumentNullException)
                    {
                        return;
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestNameProperties()
        {
            var c = new CSerializationContext();

            Assert.AreEqual( CSerializationContext.Global.ArrayAttributeName, c.ArrayAttributeName, "ArrayAttributeName" );
            c.ArrayAttributeName = "1";
            Assert.AreEqual( "1", c.ArrayAttributeName, "New ArrayAttributeName" );

            Assert.AreEqual( CSerializationContext.Global.ArrayElementName, c.ArrayElementName, "ArrayElementName" );
            c.ArrayElementName = "2";
            Assert.AreEqual( "2", c.ArrayElementName, "New ArrayElementName" );

            Assert.AreEqual( CSerializationContext.Global.ArrayLowerBoundAttribute,
                             c.ArrayLowerBoundAttribute,
                             "ArrayLowerBoundAttribute" );
            c.ArrayLowerBoundAttribute = "3";
            Assert.AreEqual( "3", c.ArrayLowerBoundAttribute, "New ArrayLowerBoundAttribute" );

            Assert.AreEqual( CSerializationContext.Global.NullAttributeName, c.NullAttributeName, "NullAttributeName" );
            c.NullAttributeName = "4";
            Assert.AreEqual( "4", c.NullAttributeName, "New NullAttributeName" );

            Assert.AreEqual( CSerializationContext.Global.NullAttributeValue, c.NullAttributeValue, "NullAttributeValue" );
            c.NullAttributeValue = "5";
            Assert.AreEqual( "5", c.NullAttributeValue, "New NullAttributeValue" );

            Assert.AreEqual( CSerializationContext.Global.RootElementName, c.RootElementName, "RootElementName" );
            c.RootElementName = "6";
            Assert.AreEqual( "6", c.RootElementName, "New RootElementName" );

            Assert.AreEqual( CSerializationContext.Global.TypeAttributeName, c.TypeAttributeName, "TypeAttributeName" );
            c.TypeAttributeName = "7";
            Assert.AreEqual( "7", c.TypeAttributeName, "New TypeAttributeName" );

            Assert.AreEqual( CSerializationContext.Global.ArrayIndexAttributeName,
                             c.ArrayIndexAttributeName,
                             "ArrayIndexAttributeName" );
            c.ArrayIndexAttributeName = "8";
            Assert.AreEqual( "8", c.ArrayIndexAttributeName, "New ArrayIndexAttributeName" );

            Assert.AreEqual( CSerializationContext.Global.ReferenceIdAttributeName,
                             c.ReferenceIdAttributeName,
                             "ReferenceIdAttributeName" );
            c.ReferenceIdAttributeName = "9";
            Assert.AreEqual( "9", c.ReferenceIdAttributeName, "New ReferenceIdAttributeName" );

            Assert.AreEqual( CSerializationContext.Global.ReferToAttributeName,
                             c.ReferToAttributeName,
                             "ReferToAttributeName" );
            c.ReferToAttributeName = "A";
            Assert.AreEqual( "A", c.ReferToAttributeName, "New ReferToAttributeName" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestConcise() => CSerializationContext.Global.SetConcise();


        [TestInitialize]
        public void ClearContext() => CSerializationContext.Global.ResetToGlobalDefault();
    }
}