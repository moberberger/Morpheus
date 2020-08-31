using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Morpheus.Standard.UnitTests.Algorithms
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class ProbabilityGeneratorTests
    {
        [TestMethod]
        public void TwoValueSimpleTest()
        {
            decimal[] vals = new decimal[] { 0, 100 };
            var pg = new ProbabilityGenerator( 45, vals );
            var probs = pg.Calculate();

            Assert.AreEqual( 0.55m, probs[0] );
            Assert.AreEqual( 0.45m, probs[1] );
        }


        [TestMethod]
        public void TwoValueCloseLimitTest()
        {
            decimal[] vals = new decimal[] { 100, 0 };
            var pg = new ProbabilityGenerator( .00001m, vals );
            var probs = pg.Calculate();

            Assert.AreEqual( 0.0000001m, probs[0] );
            Assert.AreEqual( 0.9999999m, probs[1] );
        }


        [TestMethod]
        public void TwoValueTinyErrorTest()
        {
            const decimal EXPECTED = 63m;

            decimal[] v = new decimal[] { 127, 41 };
            var pg = new ProbabilityGenerator( EXPECTED, v );
            var p = pg.Calculate();

            var sum = v[0] * p[0] + v[1] * p[1];
            var delta = Math.Abs( sum - EXPECTED );

            Assert.IsFalse( delta > 0.0000000000001m );
        }

        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void InvalidExpectedValueTest()
        {
            decimal[] vals = new decimal[] { 0, 100 };
            var pg = new ProbabilityGenerator( 450, vals );
            var probs = pg.Calculate();
        }

        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void InvalidExpectedValueTest2()
        {
            decimal[] vals = new decimal[] { -7, 100, 55, -450 };
            var pg = new ProbabilityGenerator( -500, vals );
            var probs = pg.Calculate();
        }



    }
}
