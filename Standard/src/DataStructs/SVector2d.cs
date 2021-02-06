using System;

namespace Morpheus
{
    /// <summary>
    /// An immutable 2d vector class implemented as a value-type and featuring a fluent API
    /// </summary>
    public struct SVector2d
    {
        /// <summary>
        /// Something that can be used to denote a value-type that is invalid. A reference type
        /// may use NULL, but a value type has to express this differently.
        /// </summary>
        public static readonly SVector2d NotAVector = new SVector2d( double.NaN, double.NaN );

        /// <summary>
        /// A zero-valued vector.
        /// </summary>
        public static readonly SVector2d Zero = new SVector2d( 0, 0 );

        /// <summary>
        /// The 'X' coordinate
        /// </summary>
        public readonly double X;

        /// <summary>
        /// The 'Y' coordinate
        /// </summary>
        public readonly double Y;


        /// <summary>
        /// Construct a vector with set X,Y
        /// </summary>
        /// <param name="_x">The 'X' coordinate</param>
        /// <param name="_y">The 'Y' coordinate</param>
        public SVector2d( double _x, double _y )
        {
            X = _x;
            Y = _y;
        }


        /// <summary>
        /// Is another vector the "same" as this vector? "Same" implies "really close", as
        /// opposed to "double==double"
        /// </summary>
        /// <param name="_other">The vector to compare to this one</param>
        /// <returns>TRUE if the X,Y values are "close"</returns>
        public bool AreSame( SVector2d _other ) => X.IsClose( _other.X ) && Y.IsClose( _other.Y );

        /// <summary>
        /// This is a vector when both X and Y are not NaN and they are both not Infinity
        /// </summary>
        public bool IsAVector => !double.IsInfinity( X ) &&
                       !double.IsInfinity( Y ) &&
                       !double.IsNaN( X ) &&
                       !double.IsNaN( Y );

        /// <summary>
        /// Return true when both X and Y are zero
        /// </summary>
        public bool IsZero => X == 0 && Y == 0;

        /// <summary>
        /// The Square of the Length of this vector- Also the dot-product of this vector and
        /// itself
        /// </summary>
        public double LengthSquared => X * X + Y * Y;

        /// <summary>
        /// The Length of this vector (same as "Magnitude").
        /// </summary>
        public double Length => Math.Sqrt( X * X + Y * Y );

        /// <summary>
        /// The square of the distance between this vector and another vector (assumed to be
        /// point vectors)
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>The distance squared between this and another vector</returns>
        public double DistanceSquared( SVector2d _other )
        {
            var xx = X - _other.X;
            var yy = Y - _other.Y;
            return xx * xx + yy * yy;
        }

        /// <summary>
        /// The distance between this vector and another vector
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>The distance between this and another vector</returns>
        public double Distance( SVector2d _other ) => Math.Sqrt( DistanceSquared( _other ) );

        /// <summary>
        /// Calculate the dot-product of this vector and another vector
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>The dot-product, a scalar value</returns>
        public double Dot( SVector2d _other ) => X * _other.X + Y * _other.Y;


        /// <summary>
        /// Add two vectors together and get a new SVector2d
        /// </summary>
        /// <param name="_1">The left operand</param>
        /// <param name="_2">The right operand</param>
        /// <returns>A new SVector2d resulting from the sum of two vectors</returns>
        public static SVector2d operator +( SVector2d _1, SVector2d _2 ) => new SVector2d( _1.X + _2.X, _1.Y + _2.Y );

        /// <summary>
        /// Return a vector equal to this vector plus another vector
        /// </summary>
        /// <param name="_other">The vector to add to this one</param>
        /// <returns>A vector representing the sum of two vectors</returns>
        public SVector2d AddTo( SVector2d _other ) => this + _other;

        /// <summary>
        /// Subtract two vectors together and get a new SVector2d
        /// </summary>
        /// <param name="_1">The left operand</param>
        /// <param name="_2">The right operand</param>
        /// <returns>
        /// A new SVector2d resulting from subtracting a second vector from a first vector
        /// </returns>
        public static SVector2d operator -( SVector2d _1, SVector2d _2 ) => new SVector2d( _1.X - _2.X, _1.Y - _2.Y );

        /// <summary>
        /// Return a vector equal to another vector minus this vector
        /// </summary>
        /// <param name="_other">The vector to subtract this vector from</param>
        /// <returns>
        /// A vector pointing from "this" (a position vector) to another position vector.
        /// </returns>
        public SVector2d PointTo( SVector2d _other ) => _other - this;

        /// <summary>
        /// Multiply a vector by a scalar
        /// </summary>
        /// <param name="_vector">The vector operand</param>
        /// <param name="_scale">The scalar operand</param>
        /// <returns>
        /// A new vector containing the operand multiplied by the scalar value
        /// </returns>
        public static SVector2d operator *( SVector2d _vector, double _scale ) => new SVector2d( _vector.X * _scale, _vector.Y * _scale );

        /// <summary>
        /// Scale this vector's magnitude by some amount.
        /// </summary>
        /// <param name="_scaleAmount">
        /// The amount to multiply this vector's X and Y values by, returning a new vector
        /// containing the result.
        /// </param>
        /// <returns>
        /// A new vector containing this vector's X and Y values multiplied by the scale amount
        /// </returns>
        public SVector2d ScaleBy( double _scaleAmount ) => this * _scaleAmount;

        /// <summary>
        /// Divide a vector by a scalar
        /// </summary>
        /// <param name="_vector">The vector operand</param>
        /// <param name="_scale">The scalar operand</param>
        /// <returns>A new vector containing the operand divided by the scalar value</returns>
        public static SVector2d operator /( SVector2d _vector, double _scale ) => new SVector2d( _vector.X / _scale, _vector.Y / _scale );

        /// <summary>
        /// Negate a vector
        /// </summary>
        /// <param name="_vector">The vector to negate</param>
        /// <returns>A new vector consisting of the negated vector</returns>
        public static SVector2d operator -( SVector2d _vector ) => new SVector2d( -_vector.X, -_vector.Y );

        /// <summary>
        /// Return a new vector which is this vector with both X and Y negated (multiplied by
        /// -1)
        /// </summary>
        /// <returns>
        /// A new vector which is this vector with both X and Y negated (multiplied by -1)
        /// </returns>
        public SVector2d Negate() => -this;









        /// <summary>
        /// Return a new SVector2d that has a new X value and this vector's Y value.
        /// </summary>
        /// <param name="_newX">The new X value</param>
        /// <returns>A new SVector2d that has a new X value and this vector's Y value.</returns>
        public SVector2d WithNewX( double _newX ) => new SVector2d( _newX, Y );

        /// <summary>
        /// Return a new SVector2d that has a new Y value and this vector's X value.
        /// </summary>
        /// <param name="_newY">The new Y value</param>
        /// <returns>A new SVector2d that has a new Y value and this vector's X value.</returns>
        public SVector2d WithNewY( double _newY ) => new SVector2d( X, _newY );


        /// <summary>
        /// Change the Length (Magnitude) of this vector while keeping its direction the same
        /// </summary>
        /// <param name="_newLength">The new length</param>
        /// <returns>
        /// A new vector whose length is equal to the requested length, but whose direction
        /// hasn't changed.
        /// </returns>
        public SVector2d WithNewLength( double _newLength )
        {
            var curLength = Math.Sqrt( X * X + Y * Y );
            var ratio = _newLength / curLength;

            return new SVector2d( X * ratio, Y * ratio );
        }

        /// <summary>
        /// Change the direction of the vector without changing its length/magnitude. If the
        /// length is currently zero, this will become a unit vector with the specified
        /// direction.
        /// </summary>
        /// <param name="_radians">The new direction for the vector</param>
        /// <returns>
        /// A new vector with Length equal to this vector, but direction equal to the radians.
        /// Alternately, this will return a unit vector if the currect vector is zero.
        /// </returns>
        public SVector2d WithNewDirection( double _radians )
        {
            var len = Length;
            if (len == 0)
                len = 1;

            return VectorFromRadians( _radians ) * len;
        }

        /// <summary>
        /// Create a new SVector2d that contains the unit vector for this vector
        /// </summary>
        /// <returns>A new unit vector</returns>
        public SVector2d AsUnitVector()
        {
            var len = Length;
            return new SVector2d( X / len, Y / len );
        }


        /// <summary>
        /// Calculate the dot-product of this vector and another vector. This is also equal to
        /// the cosine of the angle between the two vectors.
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>The normalized dot-product, a scalar value</returns>
        public double NormalizedDot( SVector2d _other ) => Dot( _other ) / (Length * _other.Length);

        /// <summary>
        /// Calculate the dot-product of this vector and another vector. This is also equal to
        /// the cosine of the angle between the two vectors.
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>The normalized dot-product, a scalar value</returns>
        public double CosineOfAngleBetween( SVector2d _other ) => Dot( _other ) / (Length * _other.Length);

        /// <summary>
        /// Return the angle (radians) between this and the provided vector
        /// </summary>
        /// <remarks>Another way of saying SVector2.ToRadians( _other - this )</remarks>
        /// <param name="_other"></param>
        /// <returns></returns>
        public double AngleBetween( SVector2d _other ) => Math.Atan2( _other.Y - Y, _other.X - X );

        /// <summary>
        /// Return the angle (radians) as if using the law of cosines, where "this" is the
        /// mid-point (where the interesting angle is) of the triangle formed between this and
        /// the two points provided.
        /// </summary>
        /// <param name="_first">the first one of the two other points in the triangle</param>
        /// <param name="_second">the second one of the two other points in the triangle</param>
        /// <returns></returns>
        public double AngleBetween( SVector2d _first, SVector2d _second ) => AngleBetween( _first ) - AngleBetween( _second );



        /// <summary>
        /// Determine if this and another vector are parallel
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are parallel</returns>
        public bool AreParallel( SVector2d _other )
        {
            var dot = NormalizedDot( _other );
            return dot.IsClose( 1 ) || dot.IsClose( -1 );
        }

        /// <summary>
        /// Determine if this and another vector are parallel but pointing in opposite
        /// directions
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are parallel but in opposite directions</returns>
        public bool AreParallelOppositeDir( SVector2d _other )
        {
            var dot = NormalizedDot( _other );
            return dot.IsClose( -1 );
        }

        /// <summary>
        /// Determine if this and another vector are parallel and pointing in the same direction
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are parallel and in the same direction</returns>
        public bool AreParallelSameDir( SVector2d _other )
        {
            var dot = NormalizedDot( _other );
            return dot.IsClose( 1 );
        }

        /// <summary>
        /// Determine if this and another vector are orthogonal- perpendicular
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are orthogonal</returns>
        public bool AreOrthogonal( SVector2d _other ) => NormalizedDot( _other ).IsClose( 0 );

        /// <summary>
        /// Determine if this and another vector form obtuse angles with each other
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are obtuse</returns>
        public bool AreObtuse( SVector2d _other ) => NormalizedDot( _other ) < 0;

        /// <summary>
        /// Determine if this and another vector form acute angles with each other
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are acute</returns>
        public bool AreAcute( SVector2d _other ) => NormalizedDot( _other ) > 0;

        /// <summary>
        /// Determine which "clock direction" (e.g. clockwise or counter-clockwise) the other
        /// vector is from this vector, assuming both are "rooted" at (0,0)
        /// </summary>
        /// <remarks>
        /// This is basically the calculation for the determinant of the two vectors.
        /// </remarks>
        /// <param name="_other">The other vector</param>
        /// <returns>-1: Clockwise, 1: CounterClockwise, 0: Colinear</returns>
        public int ClockDirection( SVector2d _other )
        {
            var left = X * _other.Y;
            var right = Y * _other.X;

            return (right > left) ? -1 : (left > right) ? 1 : 0;
        }


        /****************************************************************************************
         * The coordinate system for these vectors uses these quadrants:
         * 
         * 
         *                     -PI/2
         *                     
         *                       -
         * 
         *            Q3         |          Q4
         *                       |     
         *             (-1,-1)   |   (1,-1)
         *                       |     
         *                       |     
         *  PI (-PI)  -----------+------------ +   0rad
         *                       |     
         *                       |     
         *              (-1,1)   |   (1,1)   
         *                       |     
         *            Q2         |          Q1        
         * 
         *                       +
         * 
         *                      PI/2
         * 
         * 
         ***************************************************************************************/

        /// <summary>
        /// Create a unit vector from an angle specified in radians
        /// </summary>
        /// <param name="_radians">The angle to create the unit vector from</param>
        /// <returns>A new SVector2d (unit) created from the angle specified</returns>
        public static SVector2d VectorFromRadians( double _radians ) => new SVector2d( Math.Cos( _radians ), Math.Sin( _radians ) );

        /// <summary>
        /// Turn this vector (assumed to be based at (0,0)) into an angle measure.
        /// </summary>
        /// <returns>The radians (-PI to PI) for this vector</returns>
        public double ToRadians() => Math.Atan2( Y, X );


        /// <summary>
        /// Determine if this (point) vector travelling at a specific speed can intersect a
        /// second (point) vector travelling at a given linear velocity
        /// </summary>
        /// <param name="_myPosition">The position of "me"</param>
        /// <param name="_mySpeed">The speed at which this point can travel</param>
        /// <param name="_otherPosition">The position of the target point</param>
        /// <param name="_otherVelocity">The velocity of the target point</param>
        /// <param name="_interceptPosition">
        /// If interception is possible, the point of interception
        /// </param>
        /// <param name="_interceptTime">
        /// If interception is possible, the time of interception
        /// </param>
        /// <returns>
        /// The velocity vector that this point should use in order to intercept, or
        /// <see cref="SVector2d.NotAVector"/> if interception is not possible
        /// </returns>
        public static SVector2d Intercept(
            SVector2d _myPosition,
            double _mySpeed,
            SVector2d _otherPosition,
            SVector2d _otherVelocity,
            out SVector2d _interceptPosition,
            out double _interceptTime )
        {
            // First check- Are we already on top of the target? If so, its valid and we're done
            if (_myPosition.AreSame( _otherPosition ))
            {
                _interceptPosition = _myPosition;
                _interceptTime = 0;
                return SVector2d.Zero; // (0,0)
            }

            // Set "out" parameters as if a failure occurred.
            _interceptPosition = NotAVector;
            _interceptTime = double.NaN;

            // Check- Am I moving? Be gracious about exception throwing even though negative
            // speed is undefined.
            if (_mySpeed <= 0)
                return NotAVector; // No interception

            var otherSpeed = _otherVelocity.Length;
            var vectorFromOther = _myPosition - _otherPosition;
            var distanceToOther = vectorFromOther.Length;

            // Check- Is the other thing not moving? If it isn't, the calcs don't work because
            // we can't use the Law of Cosines
            if (otherSpeed.IsClose( 0 ))
            {
                _interceptPosition = _otherPosition;
                _interceptTime = distanceToOther / _mySpeed;
            }
            else // Everything looks OK for the Law of Cosines approach
            {
                var cosTheta = vectorFromOther.Dot( _otherVelocity ) / (distanceToOther * otherSpeed);

                var a = _mySpeed * _mySpeed - otherSpeed * otherSpeed;
                var b = 2 * distanceToOther * otherSpeed * cosTheta;
                var c = -distanceToOther * distanceToOther;

                if (!CMath.QuadraticSolver( a, b, c, out var t1, out var t2 ))
                    return SVector2d.NotAVector;

                if (t1 < 0 && t2 < 0)
                    return SVector2d.NotAVector;
                else if (t1 > 0 && t2 > 0)
                    _interceptTime = Math.Min( t1, t2 );
                else
                    _interceptTime = Math.Max( t1, t2 );

                _interceptPosition = _otherPosition + _otherVelocity * _interceptTime;
            }

            // Calculate the resulting velocity based on the time and intercept position
            var velocity = _interceptPosition - _myPosition;
            return velocity.WithNewLength( _mySpeed );
        }


        /// <summary>
        /// Turn this vector into a string
        /// </summary>
        /// <returns>Turn this vector into a string</returns>
        public override string ToString() => string.Format( "<{0:N3},{1:N3}>", X, Y );
    }
}
