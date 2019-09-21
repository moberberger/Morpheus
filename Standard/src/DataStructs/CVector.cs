using System;


namespace Morpheus
{
    /// <summary>
    /// Basic 2-d vector functionality
    /// </summary>
    public class CVector
    {
        /// <summary>
        /// The 'X' coordinate
        /// </summary>
        public double X;

        /// <summary>
        /// The 'Y' coordinate
        /// </summary>
        public double Y;


        /// <summary>
        /// Construct a (0,0) vector
        /// </summary>
        public CVector()
        {
        }

        /// <summary>
        /// Construct a vector with set X,Y
        /// </summary>
        /// <param name="_x">The 'X' coordinate</param>
        /// <param name="_y">The 'Y' coordinate</param>
        public CVector( double _x, double _y )
        {
            X = _x;
            Y = _y;
        }

        /// <summary>
        /// Copy constructor from another 2d vector
        /// </summary>
        /// <param name="_other">The 2d vector to copy</param>
        public CVector( CVector _other )
        {
            X = _other.X;
            Y = _other.Y;
        }


        /// <summary>
        /// Is another vector the "same" as this vector? "Same" implies "really close", as opposed to "double==double"
        /// </summary>
        /// <param name="_other">The vector to compare to this one</param>
        /// <returns>TRUE if the X,Y values are "close"</returns>
        public bool AreSame( CVector _other ) => X.IsClose( _other.X ) && Y.IsClose( _other.Y );


        /// <summary>
        /// The Square of the Length of this vector- Also the dot-product of this vector and itself
        /// </summary>
        public double LengthSquared => X * X + Y * Y;

        /// <summary>
        /// The Length of this vector (same as "Magnitude"). If Set, this vector's unit vector will be mulitplied by the new Length measure
        /// </summary>
        public double Length
        {
            get => Math.Sqrt( X * X + Y * Y );
            set
            {
                var curLength = Math.Sqrt( X * X + Y * Y );
                var ratio = value / curLength;
                X *= ratio;
                Y *= ratio;
            }
        }

        /// <summary>
        /// The Magnitude of this vector (same as "Length")
        /// </summary>
        public double Magnitude
        {
            get => Math.Sqrt( X * X + Y * Y );
            set
            {
                var curLength = Math.Sqrt( X * X + Y * Y );
                var ratio = value / curLength;
                X *= ratio;
                Y *= ratio;
            }
        }


        /// <summary>
        /// The square of the distance between this vector and another vector (assumed to be point vectors)
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>The distance squared between this and another vector</returns>
        public double DistanceSquared( CVector _other )
        {
            var xx = X - _other.X;
            var yy = Y - _other.Y;
            return xx * xx + yy * yy;
        }

        /// <summary>
        /// The distance between this vector and another vector (assumed to be point vectors)
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>The distance between this and another vector</returns>
        public double Distance( CVector _other ) => Math.Sqrt( DistanceSquared( _other ) );


        /// <summary>
        /// Add two vectors together and get a new CVector
        /// </summary>
        /// <param name="_1">The left operand</param>
        /// <param name="_2">The right operand</param>
        /// <returns>A new CVector resulting from the sum of two vectors</returns>
        public static CVector operator +( CVector _1, CVector _2 ) => new CVector( _1.X + _2.X, _1.Y + _2.Y );

        /// <summary>
        /// Subtract two vectors together and get a new CVector
        /// </summary>
        /// <param name="_1">The left operand</param>
        /// <param name="_2">The right operand</param>
        /// <returns>A new CVector resulting from subtracting a second vector from a first vector</returns>
        public static CVector operator -( CVector _1, CVector _2 ) => new CVector( _1.X - _2.X, _1.Y - _2.Y );

        /// <summary>
        /// Multiply a vector by a scalar
        /// </summary>
        /// <param name="_vector">The vector operand</param>
        /// <param name="_scale">The scalar operand</param>
        /// <returns>A new vector containing the operand multiplied by the scalar value</returns>
        public static CVector operator *( CVector _vector, double _scale ) => new CVector( _vector.X * _scale, _vector.Y * _scale );

        /// <summary>
        /// Divide a vector by a scalar
        /// </summary>
        /// <param name="_vector">The vector operand</param>
        /// <param name="_scale">The scalar operand</param>
        /// <returns>A new vector containing the operand divided by the scalar value</returns>
        public static CVector operator /( CVector _vector, double _scale ) => new CVector( _vector.X / _scale, _vector.Y / _scale );

        /// <summary>
        /// Negate a vector
        /// </summary>
        /// <param name="_vector">The vector to negate</param>
        /// <returns>A new vector consisting of the negated vector</returns>
        public static CVector operator -( CVector _vector ) => new CVector( -_vector.X, -_vector.Y );

        /// <summary>
        /// Create a new CVector that contains the unit vector for this vector
        /// </summary>
        /// <returns>A new unit vector</returns>
        public CVector ToUnitVector()
        {
            var len = Length;
            return new CVector( X / len, Y / len );
        }


        /// <summary>
        /// Add a vector to this vector, modifying this vector
        /// </summary>
        /// <param name="_other">The vector to add</param>
        public CVector Add( CVector _other )
        {
            X += _other.X;
            Y += _other.Y;
            return this;
        }

        /// <summary>
        /// Subtract a vector from this vector, modifying this vector
        /// </summary>
        /// <param name="_other">The vector to subtract</param>
        public CVector Subtract( CVector _other )
        {
            X -= _other.X;
            Y -= _other.Y;
            return this;
        }

        /// <summary>
        /// Multiply this vector by a scalar value, modifying this vector
        /// </summary>
        /// <param name="_scale">The number to multiply this vector by</param>
        public CVector Multiply( double _scale )
        {
            X *= _scale;
            Y *= _scale;
            return this;
        }

        /// <summary>
        /// Divide this vector by a scalar value, modifying this vector
        /// </summary>
        /// <param name="_scale">The number to divide this vector by</param>
        public CVector Divide( double _scale )
        {
            X /= _scale;
            Y /= _scale;
            return this;
        }

        /// <summary>
        /// Negate this vector
        /// </summary>
        public CVector Negate()
        {
            X = -X;
            Y = -Y;
            return this;
        }

        /// <summary>
        /// Turn this vector into a unit vector
        /// </summary>
        public CVector Normalize()
        {
            var len = Length;
            X /= len;
            Y /= len;
            return this;
        }

        /// <summary>
        /// Calculate the dot-product of this vector and another vector
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>The dot-product, a scalar value</returns>
        public double Dot( CVector _other ) => X * _other.X + Y * _other.Y;

        /// <summary>
        /// Calculate the dot-product of this vector and another vector- Normalize both vectors first
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>The dot-product, a scalar value</returns>
        private double NormalizedDot( CVector _other )
        {
            var l1 = Length;
            var x1 = X / l1;
            var y1 = Y / l1;

            var l2 = _other.Length;
            var x2 = _other.X / l2;
            var y2 = _other.Y / l2;

            return x1 * x2 + y1 * y2;
        }

        /// <summary>
        /// Determine if this and another vector are parallel
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are parallel</returns>
        public bool AreParallel( CVector _other )
        {
            var dot = NormalizedDot( _other );
            return dot.IsClose( 1 ) || dot.IsClose( -1 );
        }

        /// <summary>
        /// Determine if this and another vector are parallel but pointing in opposite directions
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are parallel but in opposite directions</returns>
        public bool AreParallelOppositeDir( CVector _other )
        {
            var dot = NormalizedDot( _other );
            return dot.IsClose( -1 );
        }

        /// <summary>
        /// Determine if this and another vector are parallel and pointing in the same direction
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are parallel and in the same direction</returns>
        public bool AreParallelSameDir( CVector _other )
        {
            var dot = NormalizedDot( _other );
            return dot.IsClose( 1 );
        }

        /// <summary>
        /// Determine if this and another vector are orthogonal- perpendicular
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are orthogonal</returns>
        public bool AreOrthogonal( CVector _other ) => NormalizedDot( _other ).IsClose( 0 );

        /// <summary>
        /// Determine if this and another vector form obtuse angles with each other
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are obtuse</returns>
        public bool AreObtuse( CVector _other ) => NormalizedDot( _other ) < 0;

        /// <summary>
        /// Determine if this and another vector form acute angles with each other
        /// </summary>
        /// <param name="_other">The other vector</param>
        /// <returns>TRUE if the vectors are acute</returns>
        public bool AreAcute( CVector _other ) => NormalizedDot( _other ) > 0;




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
        /// <returns>A new CVector (unit) created from the angle specified</returns>
        public static CVector VectorFromRadians( double _radians ) => new CVector( Math.Cos( _radians ), Math.Sin( _radians ) );

        /// <summary>
        /// Make this CVector a unit vector from an angle specified in radians
        /// </summary>
        /// <param name="_radians">The angle to create the unit vector from</param>
        public CVector FromRadians( double _radians )
        {
            X = Math.Cos( _radians );
            Y = Math.Sin( _radians );
            return this;
        }

        /// <summary>
        /// Turn this vector (assumed to be based at (0,0)) into an angle measure.
        /// </summary>
        /// <returns>The radians (-PI to PI) for this vector</returns>
        public double ToRadians() => Math.Atan2( Y, X );


        /// <summary>
        /// Determine if this (point) vector travelling at a specific speed can intersect a second (point) vector travelling at a given linear velocity
        /// </summary>
        /// <param name="_mySpeed">The speed at which this point can travel</param>
        /// <param name="_otherPosition">The position of the target point</param>
        /// <param name="_otherVelocity">The velocity of the target point</param>
        /// <param name="_interceptPosition">If interception is possible, the point of interception</param>
        /// <param name="_interceptTime">If interception is possible, the time of interception</param>
        /// <returns>The velocity vector that this point should use in order to intercept, or NULL if interception is not possible</returns>
        public CVector Intercept( double _mySpeed, CVector _otherPosition, CVector _otherVelocity, out CVector _interceptPosition, out double _interceptTime )
        {
            // First check- Are we already on top of the target? If so, its valid and we're done
            if (AreSame( _otherPosition ))
            {
                _interceptPosition = new CVector( this );
                _interceptTime = 0;
                return new CVector();
            }

            // Set "out" parameters as if a failure occurred.
            _interceptPosition = null;
            _interceptTime = double.NaN;

            // Check- Am I moving?
            if (_mySpeed <= 0)
                return null;

            var otherSpeed = _otherVelocity.Length;
            var vectorFromOther = this - _otherPosition;
            var distanceToOther = vectorFromOther.Length;

            // Check- Is the other thing not moving? If it isn't, the calcs don't work because we can't
            //  use the Law of Cosines
            if (otherSpeed.IsClose( 0 ))
            {
                _interceptPosition = new CVector( _otherPosition );
                _interceptTime = distanceToOther / _mySpeed;
            }
            else // Everything looks OK for the Law of Cosines approach
            {
                var cosTheta = vectorFromOther.Dot( _otherVelocity ) / (distanceToOther * otherSpeed);

                var a = _mySpeed * _mySpeed - otherSpeed * otherSpeed;
                var b = 2 * distanceToOther * otherSpeed * cosTheta;
                var c = -distanceToOther * distanceToOther;

                if (!CMath.QuadraticSolver( a, b, c, out var t1, out var t2 ))
                    return null;

                if (t1 < 0 && t2 < 0)
                    return null;
                else if (t1 > 0 && t2 > 0)
                    _interceptTime = Math.Min( t1, t2 );
                else
                    _interceptTime = Math.Max( t1, t2 );

                _interceptPosition = new CVector( _otherVelocity );
                _interceptPosition.Multiply( _interceptTime ).Add( _otherPosition );
            }

            // Calculate the resulting velocity based on the time and intercept position
            var velocity = _interceptPosition - this;
            velocity.Length = _mySpeed;

            return velocity;
        }



        /// <summary>
        /// Turn this vector into a string
        /// </summary>
        /// <returns>Turn this vector into a string</returns>
        public override string ToString() => string.Format( "({0:N3},{1:N3})", X, Y );
    }
}
