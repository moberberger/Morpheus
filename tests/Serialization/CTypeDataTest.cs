using Microsoft.VisualStudio.TestTools.UnitTesting;
using Morpheus.Serialization;
using System;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    [TestClass]
    public class CTypeDataTest : CBaseXmlPrinter
    {
        private class CExplicitConstructor
        {
            [AImplicitDeserializer]
            public CExplicitConstructor()
            {
                WasSerialized = true;
            }

            public bool WasSerialized = false;
        }

        /// <summary>
        /// Explicit constructors
        /// </summary>
        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestExplicitConstructor()
        {
            var t = CTypeData.GetTypeData( typeof( CExplicitConstructor ) );
            object s = t.ExplicitSurrogate;

            Assert.IsNotNull( s, "Expecting there to be an explicit constructor on class CExplicitConstructor" );

            t = CTypeData.GetTypeData( typeof( C6 ) );
            s = t.ExplicitSurrogate;
            Assert.IsNull( s, "Expected there to be no explicit constructor on class C6" );



            var d = new CDeserializer();
            var doc = new XmlDocument();
            doc.LoadXml( "<_ />" );
            object o = d.Deserialize<CExplicitConstructor>( doc );
            Assert.AreEqual<Type>( typeof( CExplicitConstructor ), o.GetType() );
            Assert.AreEqual<bool>( true, (o as CExplicitConstructor).WasSerialized );
        }

        /// <summary>
        /// Start testing the various classes in the hierarchy, starting with the "Top" class.
        /// </summary>
        [TestMethod]
        [TestCategory( "Serialization" )]
        public void Test6()
        {
            var t = CTypeData.GetTypeData( typeof( C6 ) );

            Assert.AreEqual( typeof( C6 ), t.Type, "Type incorrect" );
            Assert.AreEqual( typeof( C5 ), t.BaseType.Type, "Base Type incorrect" );
            Assert.AreEqual( 2, t.FieldCount, "Field Count" );

            var f = t[0];
            Assert.AreEqual( "Top", f.XmlName, "first field" );

            f = t[1];
            Assert.AreEqual( "To_2", f.XmlName, "second field" );


            f = t["Top"];
            Assert.AreEqual( typeof( int ), f.Field.FieldType, "'Top' field type-wrong" );

            f = t["To_2"];
            Assert.AreEqual( typeof( string ), f.Field.FieldType, "'To_2' field type-wrong" );


            f = t["RenameMe"];
            Assert.IsNull( f );

            f = t["NoSerializeTop"];
            Assert.IsNull( f );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void Test5()
        {
            var t = CTypeData.GetTypeData( typeof( C6 ) );
            t = t.BaseType;

            Assert.AreEqual( typeof( C5 ), t.Type, "Type incorrect" );
            Assert.AreEqual( typeof( C4 ), t.BaseType.Type, "Base Type incorrect" );
            Assert.AreEqual( 1, t.FieldCount, "Field Count" );

            var f = t[0];
            Assert.AreEqual( "Middle", f.XmlName, "first field" );

            f = t["Middle"];
            Assert.AreEqual( typeof( string ), f.Field.FieldType, "'Middle' field type-wrong" );

            f = t["NoSerializeMiddle"];
            Assert.IsNull( f );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void Test4()
        {
            var t = CTypeData.GetTypeData( typeof( C6 ) );
            t = t.BaseType;
            t = t.BaseType;

            Assert.AreEqual( typeof( C4 ), t.Type, "Type incorrect" );
            Assert.AreEqual( typeof( C3 ), t.BaseType.Type, "Base Type incorrect" );
            Assert.AreEqual( 3, t.FieldCount, "Field Count" );

            var f = t[0];
            Assert.AreEqual( "PI_double", f.XmlName, "first field" );

            f = t[1];
            Assert.AreEqual( "PIE_string", f.XmlName, "second field" );

            f = t[2];
            Assert.AreEqual( "EAT_PIE", f.XmlName, "third field" );


            f = t["PI_double"];
            Assert.AreEqual( typeof( double ), f.Field.FieldType, "'PI_double' field type-wrong" );

            f = t["PIE_string"];
            Assert.AreEqual( typeof( string ), f.Field.FieldType, "'PIE_string' field type-wrong" );

            f = t["EAT_PIE"];
            Assert.AreEqual( typeof( bool ), f.Field.FieldType, "'EAT_PIE' field type-wrong" );


            f = t["PI"];
            Assert.IsNull( f );

            f = t["PIE"];
            Assert.IsNull( f );

            f = t["something"];
            Assert.IsNull( f );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void Test3()
        {
            var t = CTypeData.GetTypeData( typeof( C6 ) );
            t = t.BaseType;
            t = t.BaseType;
            t = t.BaseType;

            Assert.AreEqual( typeof( C3 ), t.Type, "Type incorrect" );
            Assert.AreEqual( typeof( C2 ), t.BaseType.Type, "Base Type incorrect" );
            Assert.AreEqual( 3, t.FieldCount, "Field Count" );

            var f = t[0];
            Assert.AreEqual( "thisone", f.XmlName, "first field" );

            f = t[1];
            Assert.AreEqual( "thatone", f.XmlName, "second field" );

            f = t[2];
            Assert.AreEqual( "Same", f.XmlName, "second field" );

            f = t[-1];
            Assert.IsNull( f, "Expected an index outside of 0-2 to return null" );

            f = t[3];
            Assert.IsNull( f, "Expected an index outside of 0-2 to return null" );


            f = t["thisone"];
            Assert.AreEqual( typeof( int ), f.Field.FieldType, "'thisone' field type-wrong" );

            f = t["thatone"];
            Assert.AreEqual( typeof( string ), f.Field.FieldType, "'thatone' field type-wrong" );

            f = t["Same"];
            Assert.AreEqual( typeof( bool ), f.Field.FieldType, "'Same' field type-wrong" );


            f = t["THISONE"];
            Assert.IsNull( f );

            f = t["THATONE"];
            Assert.IsNull( f );

            f = t["same"];
            Assert.IsNull( f );


            CTestClassesFieldRenamer_Dynamic.UpperCase = true;


            f = t[0];
            Assert.AreEqual( "THISONE", f.XmlName, "first field" );

            f = t[1];
            Assert.AreEqual( "THATONE", f.XmlName, "second field" );

            f = t[2];
            Assert.AreEqual( "Same", f.XmlName, "second field" );


            f = t["THISONE"];
            Assert.AreEqual( typeof( int ), f.Field.FieldType, "'THISONE' field type-wrong" );

            f = t["THATONE"];
            Assert.AreEqual( typeof( string ), f.Field.FieldType, "'THATONE' field type-wrong" );

            f = t["Same"];
            Assert.AreEqual( typeof( bool ), f.Field.FieldType, "'Same' field type-wrong" );


            f = t["thisone"];
            Assert.IsNull( f );

            f = t["thatone"];
            Assert.IsNull( f );

            f = t["SAME"];
            Assert.IsNull( f );
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void Test2()
        {
            var t = CTypeData.GetTypeData( typeof( C6 ) );
            t = t.BaseType;
            t = t.BaseType;
            t = t.BaseType;
            t = t.BaseType;

            Assert.AreEqual( typeof( C2 ), t.Type, "Type incorrect" );
            Assert.AreEqual( typeof( C1 ), t.BaseType.Type, "Base Type incorrect" );
            Assert.AreEqual( 0, t.FieldCount, "Field Count" );

            var f = t[0];
            Assert.IsNull( f );

            f = t["ShouldntSerialize"];
            Assert.IsNull( f );

            f = t["AlsoNoSerialization"];
            Assert.IsNull( f );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void Test1()
        {
            var t = CTypeData.GetTypeData( typeof( C6 ) );
            t = t.BaseType;
            t = t.BaseType;
            t = t.BaseType;
            t = t.BaseType;
            t = t.BaseType;

            Assert.AreEqual( typeof( C1 ), t.Type, "Type incorrect" );
            Assert.IsNull( t.BaseType, "Base Type is not null" );
            Assert.AreEqual( 1, t.FieldCount, "Field Count" );

            var f = t[0];
            Assert.AreEqual( "ReallyAtBase", f.XmlName, "first field" );

            f = t["ReallyAtBase"];
            Assert.AreEqual( typeof( int ), f.Field.FieldType, "'ReallyAtBase' field type-wrong" );

            f = t["Top"];
            Assert.IsNull( f );
        }
    }
}
