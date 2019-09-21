using System;

namespace Morpheus
{
    /// <summary>
    /// A Line Segment, defined as having two points that describe the endpoints of said
    /// segment
    /// </summary>
    public class LineSegment
    {
        private readonly SVector2d m_point1;
        private readonly SVector2d m_point2;

        /// <summary>
        /// Construct with two vectors
        /// </summary>
        /// <param name="_point1"></param>
        /// <param name="_point2"></param>
        public LineSegment( SVector2d _point1, SVector2d _point2 )
        {
            m_point1 = _point1;
            m_point2 = _point2;
        }

        /// <summary>
        /// Construct with axis coordinates of the endpoints
        /// </summary>
        /// <param name="_x1"></param>
        /// <param name="_y1"></param>
        /// <param name="_x2"></param>
        /// <param name="_y2"></param>
        public LineSegment( double _x1, double _y1, double _x2, double _y2 )
        {
            m_point1 = new SVector2d( _x1, _y1 );
            m_point2 = new SVector2d( _x2, _y2 );
        }



        /// <summary>
        /// Determine if a point is inside the "box" formed by the endpoints of this segment
        /// </summary>
        /// <param name="_point">The point to test</param>
        /// <returns>
        /// TRUE if a point in space is inside the "box" formed by the two endpoints of this
        /// segment
        /// </returns>
        public bool HasInBox( SVector2d _point )
        {
            if (_point.X < m_point1.X && _point.X < m_point2.X)
                return false;
            if (_point.X > m_point1.X && _point.X > m_point2.X)
                return false;

            if (_point.Y < m_point1.Y && _point.Y < m_point2.Y)
                return false;
            if (_point.Y > m_point1.Y && _point.Y > m_point2.Y)
                return false;

            return true;
        }



        /// <summary>
        /// Return true if the vector AB to vector AC is counterclockwise. It is
        /// counterclockwise when the determinant is positive. Clockwise: Negative.
        /// Co-linear: 0
        /// </summary>
        /// <remarks>This is really just a determinant of vectors AB and AC</remarks>
        /// <param name="_a"></param>
        /// <param name="_b"></param>
        /// <param name="_c"></param>
        /// <returns>-1: Clockwise, 1: CounterClockwise, 0: Colinear</returns>
        private static int ClockDirection( SVector2d _a, SVector2d _b, SVector2d _c )
        {
            var left = (_c.Y - _a.Y) * (_b.X - _a.X);
            var right = (_b.Y - _a.Y) * (_c.X - _a.X);

            return left < right ? -1 : left > right ? 1 : 0;
        }

        /// <summary>
        /// Check for intercection using the "Clockwise Test". For intersection, both these
        /// conditions need to be true:
        /// 
        /// C needs to be on the "other side" of AB than D
        /// 
        /// A needs to be on the "other side" of CD than B
        /// 
        /// The "other side" test is done by checking to see if you need to go clockwise to
        /// one point and counter-clockwise to the other.
        /// </summary>
        /// <param name="_other">The LineSegment to test for intersection</param>
        /// <returns>
        /// TRUE if the segments intersect, including if (at least) part of one line segment
        /// lies on the other
        /// </returns>
        public bool DoesIntersect( LineSegment _other )
        {
            var cd1 = ClockDirection( m_point1, _other.m_point1, _other.m_point2 );
            var cd2 = ClockDirection( m_point2, _other.m_point1, _other.m_point2 );
            var cd3 = ClockDirection( m_point1, m_point2, _other.m_point1 );
            var cd4 = ClockDirection( m_point1, m_point2, _other.m_point2 );

            if (cd1 != cd2 && cd3 != cd4) // easy case
                return true;

            // now test colinearity and overlap. Only calls "HasInBox" if there's something
            // colinear about the segments
            if (cd1 == 0 && HasInBox( _other.m_point1 ))
                return true;
            if (cd2 == 0 && HasInBox( _other.m_point2 ))
                return true;
            if (cd3 == 0 && _other.HasInBox( m_point1 ))
                return true;
            if (cd4 == 0 && _other.HasInBox( m_point2 ))
                return true;

            return false;
        }


        /// <summary>
        /// User and debug friendly version
        /// </summary>
        /// <returns></returns>
        public override string ToString() => "[ " + m_point1.ToString() + ", " + m_point2.ToString() + "]";
    }
}
