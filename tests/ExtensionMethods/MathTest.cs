using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Morpheus.Standard.UnitTests
{
    /// <summary>
    /// This is a test class for CMath and is intended to contain all CMath Unit Tests
    ///</summary>
    [TestClass()]
    public class MathTest
    {
        private readonly int[] m_results = { 1, 3, 6, 10, 15 };

        [TestMethod]
        public void TestConsecutiveIntegers()
        {
            for (var i = 1; i <= m_results.Length; i++)
            {
                Assert.AreEqual<int>( m_results[i - 1], CMath.SumOfConsecutiveInts( i ), "Bad sum at index " + i );
            }
        }

        [TestMethod]
        public void TestInverseConsecutiveIntegers()
        {
            for (var i = 1; i <= m_results.Length; i++)
            {
                Assert.AreEqual<int>( i, CMath.InverseSumOfConsecutiveInts( m_results[i - 1] ), "Bad inverse sum at index " + i );
            }
        }

        [TestMethod]
        public void TestSigmoid()
        {
            var expected = .2;
            var actual = CMath.Sigmoid( 0, expected );
            Assert.AreEqual<double>( expected, actual, "For low-end of curve" );

            actual = CMath.Sigmoid( 1, expected );
            Assert.AreEqual<double>( 1 - expected, actual, "For high-end of curve" );
        }


    }
}
