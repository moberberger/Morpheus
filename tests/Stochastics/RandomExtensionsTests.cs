using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Morpheus.Standard.UnitTests.Stochastics
{
    [TestClass]
    [TestCategory( "Stochastics" )]
    public class RandomExtensions
    {
        [TestMethod]
        public void ScaleValueTest()
        {
            ulong x = ulong.MaxValue;
            ulong max = 37;

            ulong scaled = x.ScaleValue( max, ulong.MaxValue, out bool isValid );
            Assert.IsFalse( isValid );

            var expected = x % max;
            Assert.AreEqual( scaled, ulong.MaxValue % max );
        }

        [TestMethod]
        public void ScaleValue_OverLimitTest()
        {
            ulong x = 0xffff_ffff_ffff_fff4;
            ulong max = 37;

            ulong scaled = x.ScaleValue( max, ulong.MaxValue, out bool isValid );
            Assert.IsFalse( isValid );

            var expected = x % max;
            Assert.AreEqual( scaled, expected );
        }

        [TestMethod]
        public void ScaleValue_AtLimitTest()
        {
            ulong x = 0xffff_ffff_ffff_fff3;
            ulong max = 37;

            ulong scaled = x.ScaleValue( max, ulong.MaxValue, out bool isValid );
            Assert.IsTrue( isValid );

            var expected = x % max;
            Assert.AreEqual( scaled, expected );
        }


        [TestMethod]
        public void FromIntegersTest()
        {
            uint counter = 1;
            byte[] array = new byte[11];

            array.FromIntegers( () => counter++ );

            byte[] expected = new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0 };

            array.Collate( expected, ( x, y ) => Assert.AreEqual( x, y ) );
        }



        [TestMethod]
        public void FromIntegers2Test()
        {
            ulong counter = 1;
            byte[] array = new byte[11];

            array.FromIntegers( () => counter++ );

            byte[] expected = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0 };

            array.Collate( expected, ( x, y ) => Assert.AreEqual( x, y ) );
        }



        [TestMethod]
        public void LerpZeroToOneTest()
        {
            ulong zero = ulong.MinValue;
            ulong big = ulong.MaxValue;
            ulong one = 1L << 12;

            Assert.AreEqual( 0.0, zero.LerpZeroToOne() );
            Assert.IsTrue( one.LerpZeroToOne() > 0.0 );
            Assert.IsTrue( big.LerpZeroToOne() < 1.0 );
        }


        [TestMethod]
        public void ScaleValue_32_Test()
        {
            // This is a little overloaded- it covers all the cases within ScaleValue(this
            // Random, ulong)
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );
            writer.Write( uint.MaxValue ); // cause a biased number if maxValue is even
            writer.Write( 0x0000_0017 );

            var buf = mem.ToArray();
            var rng = new NotRandomFromBytes( buf );
            var rnga = rng.Aspect();
            var actual = rnga.Next( 0x10 );

            Assert.AreEqual( 7, actual );

            rng = new NotRandomFromBytes( buf ); // start over
            actual = (int) rng.ScaleValue( 0x10 );

            Assert.AreEqual( 7, actual );
        }



        [TestMethod]
        public void ScaleValue_64_Test()
        {
            // This is a little overloaded- it covers all the cases within ScaleValue(this
            // Random, ulong)
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );
            writer.Write( ulong.MaxValue ); // cause a biased number if maxValue is even
            writer.Write( 0x0000_0001_0000_0017 );

            var buf = mem.ToArray();
            var rng = new NotRandomFromBytes( buf );
            var rnga = rng.Aspect();
            var actual = rnga.NextLong( 0x10 );

            Assert.AreEqual( 7, actual );

            rng = new NotRandomFromBytes( buf ); // start over
            actual = (long) rng.ScaleValue( 0x10L );

            Assert.AreEqual( 7, actual );

            var zero = 0ul;
            var z2 = zero.ScaleValue( 1, ulong.MaxValue, out _ );
            Assert.AreEqual( zero, z2 );
        }



        [TestMethod]
        [ExpectedException( typeof( EndOfStreamException ) )]
        public void Gaussian_HiRsq_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );

            writer.Write( 0.0 );
            writer.Write( 0.0 );

            var rng = new NotRandomFromBytes( mem.ToArray() );
            var actual = rng.NextGaussian(); // should throw because rsq values are too high
        }

        [TestMethod]
        [ExpectedException( typeof( EndOfStreamException ) )]
        public void Gaussian_ZeroRsq_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );

            writer.Write( 0.5 );
            writer.Write( 0.5 );

            var rng = new NotRandomFromBytes( mem.ToArray(), true );
            var actual = rng.NextGaussian(); // should throw because rsq values are zero
        }


        [TestMethod]
        public void Gaussian_KnownValue_Test()
        {
            var mem = new MemoryStream();
            var writer = new BinaryWriter( mem );

            var d1 = 0.25;
            var d2 = 0.75;

            writer.Write( d1 );
            writer.Write( d2 );

            var rng = new NotRandomFromBytes( mem.ToArray(), true );
            var actual = rng.NextGaussian(); 

            var r1 = d1 * 2 - 1;
            var r2 = d2 * 2 - 1;
            var rsq = r1 * r1 + r2 * r2;
            var expected = r1 * Math.Sqrt( -2.0 * Math.Log( rsq ) / rsq );

            Assert.AreEqual( expected, actual );
        }

    }
}
