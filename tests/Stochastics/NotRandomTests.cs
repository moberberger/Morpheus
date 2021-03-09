using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.Standard.UnitTests.Stochastics
{
    [TestClass]
    [TestCategory( "Stochastics" )]
    public class NotRandomExtensionsTests
    {
        //[TestMethod]
        //public void NotRandomIntegerTests()
        //{
        //    var mem = new MemoryStream();
        //    var writer = new BinaryWriter( mem );

        //    writer.Write( uint.MinValue );
        //    writer.Write( uint.MaxValue );
        //    writer.Write( ulong.MinValue );
        //    writer.Write( ulong.MaxValue );

        //    var notRandomBase = new NotRandomFromBytes( mem.ToArray() );
        //    var notRandom = notRandomBase.Aspect();

        //    var x1 = notRandom.Next32();
        //    Assert.AreEqual( uint.MinValue, x1 );
        //    x1 = notRandom.Next32();
        //    Assert.AreEqual( uint.MaxValue, x1 );

        //    var x2 = notRandom.Next64();
        //    Assert.AreEqual( ulong.MinValue, x2 );
        //    x2 = notRandom.Next64();
        //    Assert.AreEqual( ulong.MaxValue, x2 );
        //}


        [TestMethod]
        public void NotRandomDouble_Ratio_Test()
        {
            var mem = new MemoryStream();
            using (var writer = new BinaryWriter( mem ))
            {
                writer.Write( (ulong)0 );
                writer.Write( (ulong)0x4000_0000_0000_0000 );
                writer.Write( (ulong)0x8000_0000_0000_0000 );
                writer.Write( (ulong)0xc000_0000_0000_0000 );
                writer.Write( (ulong)0xffff_ffff_ffff_ffff );

                var notRandom = new NotRandomFromBytes( mem.ToArray() );

                var x = notRandom.NextDouble();
                Assert.AreEqual( 0.0, x );

                x = notRandom.NextDouble();
                Assert.AreEqual( 0.25, x );

                x = notRandom.NextDouble();
                Assert.AreEqual( 0.5, x );

                x = notRandom.NextDouble();
                Assert.AreEqual( 0.75, x );

                x = notRandom.NextDouble();
                Assert.IsTrue( x < 1.0 );
            }
        }



    }
}
