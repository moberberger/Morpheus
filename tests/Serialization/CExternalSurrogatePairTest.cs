using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    [TestClass]
    public class CExternalSurrogatePairTest
    {
        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestConstructor()
        {
            IExternalSurrogate s1 = new CGuidSurrogate();
            IExternalSurrogate s2 = new CDateTimeSurrogate();

            var pair = new CExternalSurrogatePair( s1, s2 );

            Assert.AreSame( s1, pair.Surrogate1, "Surrogate1 does not point to the same object" );
            Assert.AreSame( s2, pair.Surrogate2, "Surrogate2 does not point to the same object" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestUpdate()
        {
            IExternalSurrogate s1 = new CGuidSurrogate();
            IExternalSurrogate s2 = new CDateTimeSurrogate();
            IExternalSurrogate s3 = new CUtcDateTimeSurrogate();

            IExternalSurrogate accum = null;

            accum = CExternalSurrogatePair.Update( accum, null );
            Assert.IsNull( accum, "Updating NULL with NULL should result in NULL" );

            accum = CExternalSurrogatePair.Update( accum, s1 );
            Assert.AreSame( s1, accum, "After UPDATEing null with s1, the accumulator should be s1" );

            accum = CExternalSurrogatePair.Update( accum, null );
            Assert.AreSame( s1, accum, "After UPDATEing the s1 accumulator with NULL, the accumulator should not have changed" );


            accum = CExternalSurrogatePair.Update( accum, s2 );
            Assert.AreEqual( typeof( CExternalSurrogatePair ), accum.GetType(), "After UPDATEing accumulator with s2, the accumulator should now be a surrogate Pair." );

            Assert.AreSame( s1, (accum as CExternalSurrogatePair).Surrogate1, "The first surrogate of the Pair should be s1" );
            Assert.AreSame( s2, (accum as CExternalSurrogatePair).Surrogate2, "The second surrogate of the Pair should be s2" );


            accum = CExternalSurrogatePair.Update( accum, s3 );
            Assert.AreEqual( typeof( CExternalSurrogatePair ), accum.GetType(), "After UPDATEing accumulator with s3, the accumulator should still be a surrogate Pair." );

            Assert.AreEqual( typeof( CExternalSurrogatePair ), (accum as CExternalSurrogatePair).Surrogate1.GetType(), "The Type of the first surrogate of the Pair should be a Pair" );
            Assert.AreEqual( typeof( CUtcDateTimeSurrogate ), (accum as CExternalSurrogatePair).Surrogate2.GetType(), "The Type of the second surrogate of the Pair should be the UTC Time surrogate" );
        }

        #region ClassesForChainTesting
        public class CTestBase
        {
            public static int SER_STATUS = 0;
            public static int DESER_STATUS = 0;
        }

        public class CTest1 : CTestBase, IExternalSurrogate
        {
            public bool Serialize( object _object, Type _useType, XmlElement _parentElement, CSerializer _serializer )
            {
                SER_STATUS |= 1;
                return false;
            }

            public bool Deserialize( CWorkingObject _workingObject, XmlElement _parentElement, CDeserializer _deserializer )
            {
                DESER_STATUS |= 1;
                return false;
            }
        }

        public class CTest2 : CTestBase, IExternalSurrogate
        {
            public bool Serialize( object _object, Type _useType, XmlElement _parentElement, CSerializer _serializer )
            {
                SER_STATUS |= 2;
                return false;
            }

            public bool Deserialize( CWorkingObject _workingObject, XmlElement _parentElement, CDeserializer _deserializer )
            {
                DESER_STATUS |= 2;
                return false;
            }
        }

        public class CTest3 : CTestBase, IExternalSurrogate
        {
            public bool Serialize( object _object, Type _useType, XmlElement _parentElement, CSerializer _serializer )
            {
                SER_STATUS |= 4;
                return false;
            }

            public bool Deserialize( CWorkingObject _workingObject, XmlElement _parentElement, CDeserializer _deserializer )
            {
                DESER_STATUS |= 4;
                return false;
            }
        }

        public class CTest4 : CTestBase, IExternalSurrogate
        {
            public bool Serialize( object _object, Type _useType, XmlElement _parentElement, CSerializer _serializer )
            {
                SER_STATUS |= 8;
                return false;
            }

            public bool Deserialize( CWorkingObject _workingObject, XmlElement _parentElement, CDeserializer _deserializer )
            {
                DESER_STATUS |= 8;
                return false;
            }
        }
        #endregion

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestChainSerialize()
        {
            IExternalSurrogate a = new CTest1();

            a = CExternalSurrogatePair.Update( a, new CTest2() );
            a = CExternalSurrogatePair.Update( a, new CTest4() );

            Assert.AreEqual( 0, CTestBase.SER_STATUS, "Initial SER_STATUS should be zero" );
            Assert.AreEqual( 0, CTestBase.DESER_STATUS, "Initial DESER_STATUS should be zero" );


            a.Serialize( null, null, null, null );
            Assert.AreEqual( 11, CTestBase.SER_STATUS, "After serializing the first time, the SER_STATUS is wrong" ); // 00001011 binary == 11 base 10
            Assert.AreEqual( 0, CTestBase.DESER_STATUS, "After serializing the first time, the DESER_STATUS should still be zero" );

            a.Deserialize( null, null, null );
            Assert.AreEqual( 11, CTestBase.SER_STATUS, "After deserializing the first time, the SER_STATUS should not have changed" ); // 00001011 binary == 11 base 10
            Assert.AreEqual( 11, CTestBase.DESER_STATUS, "After deserializing the first time, the DESER_STATUS is wrong" );



            a = CExternalSurrogatePair.Update( a, new CTest3() );
            CTestBase.SER_STATUS = 0;
            CTestBase.DESER_STATUS = 0;

            a.Serialize( null, null, null, null );
            Assert.AreEqual( 15, CTestBase.SER_STATUS, "After serializing the first time, the SER_STATUS is wrong" ); // 00001111 binary == 15 base 10
            Assert.AreEqual( 0, CTestBase.DESER_STATUS, "After serializing the first time, the DESER_STATUS should still be zero" );

            a.Deserialize( null, null, null );
            Assert.AreEqual( 15, CTestBase.SER_STATUS, "After deserializing the first time, the SER_STATUS should not have changed" ); // 00001111 binary == 15 base 10
            Assert.AreEqual( 15, CTestBase.DESER_STATUS, "After deserializing the first time, the DESER_STATUS is wrong" );
        }

        [TestMethod]
        [TestCategory( "Serialization" )]
        [ExpectedException( typeof( ArgumentNullException ), "Expected ArgNullEx from passing the constructor a null" )]
        public void TestNullConstructorParam1()
        {
            IExternalSurrogate s = new CExternalSurrogatePair( null, null );
        }
    }
}
