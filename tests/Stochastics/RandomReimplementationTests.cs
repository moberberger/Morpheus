using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.Standard.UnitTests.Stochastics
{
    [TestClass]
    [TestCategory( "Stochastics" )]
    public class RandomReImplementationTests
    {
        [TestMethod]
        [ExpectedException( typeof( EndOfStreamException ) )]
        public void Random_Next0a_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );
            writer.Write( uint.MaxValue );

            var rng = new NotRandomFromBytes( mem.ToArray() );
            var rnga = rng.Aspect();

            // now we have an Aspect wrapping exactly 4 0xff bytes. This will cause the Aspect's
            // Next operation to request more bytes (as the value will == int.MaxValue), and
            // cause the stream to fail.
            var actual = rnga.Next();
        }


        [TestMethod]
        public void Random_Next0b_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );
            writer.Write( uint.MaxValue );
            writer.Write( 5 );

            var rng = new NotRandomFromBytes( mem.ToArray() );
            var rnga = rng.Aspect();

            // now we have an Aspect wrapping exactly 4 0xff bytes followed by 4 bytes equalling
            // 5. The Aspect's Next operation will request more bytes (as the first value will
            // == int.MaxValue), and this time the stream will not fail.
            var actual = rnga.Next();

            Assert.AreEqual( 5, actual );
        }


        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void Random_Next1a_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );
            writer.Write( uint.MaxValue );

            var rng = new NotRandomFromBytes( mem.ToArray() );
            var rnga = rng.Aspect();

            // now we have an Aspect wrapping exactly 4 0xff bytes . The Aspect's Next operation
            // will request more bytes (as the first value will == int.MaxValue), and this time
            // the stream will not fail.
            var actual = rnga.Next( -1 );
        }


        [TestMethod]
        public void Random_Next1b_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );
            writer.Write( uint.MaxValue );
            writer.Write( 42 );

            var rng = new NotRandomFromBytes( mem.ToArray() );
            var rnga = rng.Aspect();

            var actual = rnga.Next( 30 );
            var expected = 42 % 30;
            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void Random_Next2_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );
            writer.Write( uint.MaxValue ); // will cause bias
            writer.Write( uint.MaxValue - 1 ); // will be returned

            var rng = new NotRandomFromBytes( mem.ToArray() );
            var rnga = rng.Aspect();

            var actual = rnga.Next( int.MinValue, int.MaxValue );
            var expected = int.MaxValue - 1;
            Assert.AreEqual( expected, actual );
        }

        [TestMethod]
        public void Random_NextLong2_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );
            writer.Write( ulong.MaxValue ); // will cause bias
            writer.Write( ulong.MaxValue - 1 ); // will be returned

            var rng = new NotRandomFromBytes( mem.ToArray() );
            var rnga = rng.Aspect();

            var actual = rnga.NextLong( long.MinValue, long.MaxValue );
            var expected = long.MaxValue - 1;
            Assert.AreEqual( expected, actual );
        }




        [TestMethod]
        public void Random_double_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );
            writer.Write( 1L << 63 ); // Exactly in the middle [0..uint.MaxValue+1]

            var rng = new NotRandomFromBytes( mem.ToArray() );
            var rnga = rng.Aspect();

            var actual = rnga.NextDouble();

            Assert.AreEqual( 0.5, actual );
        }


    }
}
