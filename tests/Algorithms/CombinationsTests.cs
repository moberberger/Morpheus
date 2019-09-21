using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.Standard.UnitTests.Algorithms
{
    [TestClass]
    [TestCategory( "Algorithms" )]
    public class ForEachTests
    {
        [TestMethod]
        public void CombinationsTest()
        {
            var expected = new HashSet<string> { "01", "12", "02" };

            // Should return all combinations of {0, 1, 2}
            foreach (var x in Combinations.Integers( 3, 2 ))
            {
                int lower = Math.Min( x[0], x[1] );
                int higher = Math.Max( x[0], x[1] );
                var str = $"{lower}{higher}";

                if (expected.Contains( str ))
                    expected.Remove( str );
                else
                    Assert.Fail( $"{str} was not in the set when it should be" );
            }

            Assert.AreEqual( 0, expected.Count );
        }

        [TestMethod]
        public void PermutationsTest()
        {
            var expected = new HashSet<string>
            {
                "01", "12", "02",
                "10", "21", "20",
            };

            // Should return all combinations of {0, 1, 2}
            foreach (var x in Permutations.Integers( 3, 2 ))
            {
                int first = x[0];
                int second = x[1];
                var str = $"{first}{second}";

                if (expected.Contains( str ))
                    expected.Remove( str );
                else
                    Assert.Fail( $"{str} was not in the set when it should be" );
            }

            Assert.AreEqual( 0, expected.Count );
        }
    }
}
