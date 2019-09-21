using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Xml;

namespace Morpheus.Standard.UnitTests.Serialization
{
    [TestClass]
    public class CAutoBackedPropertiesTest
    {
        public class CClassWithAutoBackedProperties
        {
            public int Age { get; set; }
            public string Nickname { get; set; }
        }


        [TestMethod]
        [TestCategory( "Serialization" )]
        public void AutoBackedPropertiesTest()
        {
            // XmlException: '.l.Nickname.g.k__BackingField' is not a valid XML Name
            //                .l.Nickname.g.k__BackingField>Snorg</.l.Nickname.g.k__BackingField></_Root>


            var obj = new CClassWithAutoBackedProperties() { Age = 45, Nickname = "Snorg" };

            var ser = new CSerializer();
            var xml = ser.Serialize( obj );

            Console.WriteLine( xml.OuterXml );

            var deser = new CDeserializer();
            var x = deser.Deserialize<CClassWithAutoBackedProperties>( xml );

            Assert.AreEqual( obj.Nickname, x.Nickname, "Name" );
            Assert.AreEqual( obj.Age, x.Age, "Age" );
        }

    }
}
