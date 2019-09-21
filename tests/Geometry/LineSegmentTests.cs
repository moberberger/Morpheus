using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Morpheus.Standard.UnitTests
{
    [TestClass]
    public class LineSegmentTests
    {
        [TestMethod]
        [TestCategory( "Geometry" )]
        public void LS_NoIntersection1()
        {
            var ls1 = new LineSegment( 0, 0, 10, 4 );
            var ls2 = new LineSegment( 6, 2, 7, 2 );

            Assert.IsFalse( ls1.DoesIntersect( ls2 ), "NoIntersect1 1-2" );
            Assert.IsFalse( ls2.DoesIntersect( ls1 ), "NoIntersect1 2-1" );
        }

        [TestMethod]
        [TestCategory( "Geometry" )]
        public void LS_NoIntersection2()
        {
            var ls1 = new LineSegment( 0, 0, 10, 4 );
            var ls2 = new LineSegment( 6, 2, 20, 2 );

            Assert.IsFalse( ls1.DoesIntersect( ls2 ), "NoIntersect2 1-2" );
            Assert.IsFalse( ls2.DoesIntersect( ls1 ), "NoIntersect2 2-1" );
        }

        [TestMethod]
        [TestCategory( "Geometry" )]
        public void LS_ParallelNoIntersection()
        {
            var ls1 = new LineSegment( 0, 0, 2, 0 );
            var ls2 = new LineSegment( 1, 1, 3, 1 );

            Assert.IsFalse( ls1.DoesIntersect( ls2 ), "LS_ParallelNoIntersection 1-2" );
            Assert.IsFalse( ls2.DoesIntersect( ls1 ), "LS_ParallelNoIntersection 2-1" );
        }

        [TestMethod]
        [TestCategory( "Geometry" )]
        public void LS_SimpleIntersection()
        {
            var ls1 = new LineSegment( -1, 0, 1, 0 );
            var ls2 = new LineSegment( 0, 1, 0, -1 );

            Assert.IsTrue( ls1.DoesIntersect( ls2 ), "LS_SimpleIntersection 1-2" );
            Assert.IsTrue( ls2.DoesIntersect( ls1 ), "LS_SimpleIntersection 2-1" );
        }



        [TestMethod]
        [TestCategory( "Geometry" )]
        public void LS_EndpointOnSegment()
        {
            var ls1 = new LineSegment( 0, 0, 2, 0 );
            var ls2 = new LineSegment( 1, 0, 1, 4 );

            Assert.IsTrue( ls1.DoesIntersect( ls2 ), "LS_EndpointOnSegment 1-2" );
            Assert.IsTrue( ls2.DoesIntersect( ls1 ), "LS_EndpointOnSegment 2-1" );
        }


        [TestMethod]
        [TestCategory( "Geometry" )]
        public void LS_EndpointsTouching()
        {
            var ls1 = new LineSegment( 0, 0, 2, 0 );
            var ls2 = new LineSegment( 2, 0, 2, 4 );

            Assert.IsTrue( ls1.DoesIntersect( ls2 ), "LS_EndpointsTouching 1-2" );
            Assert.IsTrue( ls2.DoesIntersect( ls1 ), "LS_EndpointsTouching 2-1" );
        }

        [TestMethod]
        [TestCategory( "Geometry" )]
        public void LS_EndpointColinearNotTouching()
        {
            var ls1 = new LineSegment( 0, 0, 2, 0 );
            var ls2 = new LineSegment( 3, 0, 3, 1 );

            Assert.IsFalse( ls1.DoesIntersect( ls2 ), "LS_EndpointColinearNotTouching 1-2" );
            Assert.IsFalse( ls2.DoesIntersect( ls1 ), "LS_EndpointColinearNotTouching 2-1" );
        }




        [TestMethod]
        [TestCategory( "Geometry" )]
        public void LS_ColinearNotTouching()
        {
            var ls1 = new LineSegment( 0, 0, 2, 2 );
            var ls2 = new LineSegment( 3, 3, 5, 5 );

            Assert.IsFalse( ls1.DoesIntersect( ls2 ), "LS_ColinearNotTouching 1-2" );
            Assert.IsFalse( ls2.DoesIntersect( ls1 ), "LS_ColinearNotTouching 2-1" );
        }


        [TestMethod]
        [TestCategory( "Geometry" )]
        public void LS_ColinearOverlapping()
        {
            var ls1 = new LineSegment( 0, 0, 2, 0 );
            var ls2 = new LineSegment( 1, 0, 3, 0 );

            Assert.IsTrue( ls1.DoesIntersect( ls2 ), "LS_ColinearOverlapping 1-2" );
            Assert.IsTrue( ls2.DoesIntersect( ls1 ), "LS_ColinearOverlapping 2-1" );

            ls1 = new LineSegment( 1, 1, 3, 3 );
            ls2 = new LineSegment( 2, 2, 0, 0 );

            Assert.IsTrue( ls1.DoesIntersect( ls2 ), "LS_ColinearOverlapping 1-2" );
            Assert.IsTrue( ls2.DoesIntersect( ls1 ), "LS_ColinearOverlapping 2-1" );
        }
    }
}
