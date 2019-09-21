using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace Morpheus.Standard.UnitTests
{
    [TestClass]
    public class CCRC16Test
    {
        [TestMethod]
        public void TestUshort()
        {
            ushort x = 0xa55a;
            var crc = CCrc16_CCITT.CalculateCrc( x );

            Assert.AreEqual( 54119, crc, "The output of the CRC is wrong for a ushort" );
        }

        [TestMethod]
        public void TestBuffer()
        {
            byte[] buf = { 0x5a, 0xa5 };
            var crc = CCrc16_CCITT.CalculateCrc( buf );

            Assert.AreEqual( 54119, crc, "The output of the CRC is wrong for a ushort" );
        }
    }
}
