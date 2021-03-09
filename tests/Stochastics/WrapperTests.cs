#if false

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Morpheus.Standard.UnitTests.Stochastics
{
    [TestClass]
    [TestCategory( "Stochastics" )]
    public class RandomWrappers
    {
        [TestMethod]
        public void ToThreadsafeTest()
        {
            Random baseline = new Random( 0 );
            var baseBuf = new byte[8];
            baseline.NextBytes( baseBuf );
            var baseValue = BitConverter.ToUInt64( baseBuf );

            Random testRng = new Random( 0 );
            var wrapped = testRng.Threadsafe();
            Assert.AreNotEqual( testRng, wrapped );

            var aspectValue = wrapped.Next64();

            Assert.AreEqual( baseValue, aspectValue );
        }


        [TestMethod]
        public void ToAspectTest()
        {
            Random baseline = new Random( 0 );
            var baseBuf = new byte[8];
            baseline.NextBytes( baseBuf );
            var baseValue = BitConverter.ToUInt64( baseBuf );

            Random testRng = new Random( 0 );
            var wrapped = testRng.Threadsafe();
            Assert.AreNotEqual( testRng, wrapped );

            var aspectValue = wrapped.Next64();

            Assert.AreEqual( baseValue, aspectValue );
        }

        [TestMethod]
        public void RandomWrapper_threadsafe_Test()
        {
            // make sure wrappers don't wrap themselves
            var r1 = new Random();
            var rth1 = r1.Threadsafe();
            var r2 = (Random) rth1;
            var rth2 = r2.Threadsafe(); // this should be wrapping a threadsafe with a threadsafe- This should not result in 2 wrappers.

            Assert.AreEqual( rth1, rth2 );
        }

        [TestMethod]
        public void RandomWrapper_aspect_Test()
        {
            // make sure wrappers don't wrap themselves
            var r1 = new Random();
            var rasp1 = r1.Aspect();
            var r2 = (Random) rasp1;
            var rasp2 = r2.Aspect(); // this should be wrapping an Aspect with another Aspect- This should not result in 2 wrappers.

            Assert.AreEqual( rasp1, rasp2 );
        }

        [TestMethod]
        public void RandomWrapper_both_Test()
        {
            var r = new Random();
            var rth = r.Threadsafe();
            var rasp = r.Aspect();

            Assert.AreNotEqual( rth, rasp );

            var rasp2 = rth.Aspect(); // Threadsafe is already an Aspect, so this should return itself
            Assert.AreEqual( rasp2, rth );

            var rth2 = rasp.Threadsafe(); // Not the other way around in this case
            Assert.AreNotEqual( rth2, rasp );

            // now rth2 is a Threadsafe wrapping an Aspect wrapping Random.
            var rasp3 = rth2.Aspect();
            Assert.AreEqual( rasp3, rth2 );
        }
    }
}
#endif