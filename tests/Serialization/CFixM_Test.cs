using Microsoft.VisualStudio.TestTools.UnitTesting;
using Morpheus.Serialization;
using System.Reflection;

namespace Morpheus.Standard.UnitTests.Serialization
{
    /// <summary>
    /// This is a test class for Oberberger.Morpheus.CFixM_ and is intended
    /// to contain all Oberberger.Morpheus.CFixM_ Unit Tests
    ///</summary>
    [TestClass]
    public class CFixM_Test
    {
        [TestMethod]
        public void TestConvertName()
        {
            var fieldNames = new string[] { "Name", "address", "m_age", "m_x", "m_", "m_Name", "m_1" };
            var correctNames = new string[] { "Name", "address", "Age", "X", "m_", "m_Name", "m_1" };

            for (var i = 0; i < fieldNames.Length; i++)
            {
                var fn = fieldNames[i];
                var expected = correctNames[i];

                var actual = CFixM_.ConvertName( fn );
                Assert.AreEqual( expected, actual, "The converted name is incorrect" );
            }
        }

        public int m_someField;

        [TestMethod]
        [TestCategory( "Serialization" )]
        public void TestConstructor()
        {
            var fld = GetType().GetField( "m_someField" );
            var fixer = new CFixM_();
            var actual = fixer.ConvertFieldName( fld.Name, fld );

            Assert.AreEqual( "SomeField", actual, "The converted field name does not match" );
        }
    }
}
