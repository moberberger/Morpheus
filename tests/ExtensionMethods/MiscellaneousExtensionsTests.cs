using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Morpheus.Standard.UnitTests.Extensions
{
    [TestClass]
    [TestCategory( "Extensions" )]
    public class MiscellaneousExtensionsTests
    {
        [TestMethod]
        public void SwapTest()
        {
            string x = "Alpha";
            string y = "Beta";

            x = x.Swap( ref y );

            Assert.AreEqual( "Beta", x );
            Assert.AreEqual( "Alpha", y );
        }

        [TestMethod]
        public void LoopInfinitelyTest()
        {
            var items = new int[] { 1, 2, 3 };
            int count = 0;
            int sum = 0;

            foreach (var x in items.LoopInfinitely())
            {
                count++;
                sum += x;

                if (count == 5) break;
            }

            // 1 + 2 + 3 + 1 + 2
            Assert.AreEqual( 9, sum );
        }


    }
}
