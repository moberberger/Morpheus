using Microsoft.VisualStudio.TestTools.UnitTesting;
using Morpheus.Serialization;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    [TestClass]
    public class CGenericDerivativeTest : CBaseXmlPrinter
    {
        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestDerivative()
        {
            var list = new CDerivedFromGenericList
            {
                X = 55
            };
            list.Add( "homer" );
            list.Add( "lisa" );

            var s = new CSerializer();
            var doc = s.Serialize( list );

            Print( doc );

            var d = new CDeserializer();
            var list2 = (CDerivedFromGenericList) d.Deserialize( doc );

            Assert.AreEqual( 55, list2.X, "X was deserialized wrong" );
            Assert.AreEqual( 2, list2.Count, "Count is wrong" );
            Assert.AreEqual( "homer", list2[0], "item[0] is wrong" );
            Assert.AreEqual( "lisa", list2[1], "item[1] is wrong" );
        }

        public class CPersonWithObject<T> : CPerson
        {
            public T SomeObject;
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestGenericDerived()
        {
            var src = new CPersonWithObject<CAddress>()
            {
                SomeObject = new CAddress()
                {
                    m_city = "Spring",
                    m_street = "Halifax",
                    m_zip = 37174
                }
            };

            var s = new CSerializer();
            var xml = s.Serialize( src );

            Print( xml );



            var d = new CDeserializer();
            var dest = d.Deserialize( xml ) as CPersonWithObject<CAddress>;

            Assert.AreEqual( src.m_name, dest.m_name, "Name" );
            Assert.AreEqual( src.SomeObject.m_street, dest.SomeObject.m_street, "Generic Street" );
            Assert.AreEqual( src.m_address.m_zip, dest.m_address.m_zip, "Zip" );
            Assert.AreNotEqual( src.SomeObject, dest.SomeObject );
        }
    }
}
