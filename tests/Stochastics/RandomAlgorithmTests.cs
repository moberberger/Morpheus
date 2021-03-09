using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.Standard.UnitTests.Stochastics
{
    [TestClass]
    [TestCategory( "Stochastics" )]
    public class RandomAlgorithmTests
    {
        [TestMethod]
        public void MersenneDisposeTest()
        {
            using (var rng = new MersenneTwister())
                Assert.AreNotEqual( int.MaxValue, rng.Next() );
        }


        [TestMethod]
        public void RandomSeed_Robust_Test()
        {
            Assert.AreNotEqual( RandomSeed.Robust(), RandomSeed.Robust() );

            var buf = new byte[1000];
            Rng.Default.NextBytes( buf );

            var first = RandomSeed.Robust( buf );
            var second = RandomSeed.Robust( buf );
            Assert.AreNotEqual( first, second );
        }

        [TestMethod]
        public void CryptoDisposeTest()
        {
            using (var rng = new CryptoRandomNumbers())
                Assert.AreNotEqual( int.MaxValue, rng.Next() );
        }


    }
}
