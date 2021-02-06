using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus
{
    /// <summary>
    /// A class containing a variety of Math helper functions
    /// </summary>
    public static class CMath
    {
        /// <summary>
        /// The natural logarithm of 2
        /// </summary>
        public static readonly double NaturalLogOf2 = Math.Log( 2.0 );

        /// <summary>
        /// The square root of 2
        /// </summary>
        public static readonly double SquareRootOf2 = Math.Sqrt( 2 );

        /// <summary>
        /// PI
        /// </summary>
        public const double PI = Math.PI;

        /// <summary>
        /// PI over 2 (half of PI)
        /// </summary>
        public const double PI_OVER_2 = Math.PI / 2;

        /// <summary>
        /// PI over 4
        /// </summary>
        public const double PI_OVER_4 = Math.PI / 4;

        /// <summary>
        /// Two times PI
        /// </summary>
        public const double TWO_PI = Math.PI * 2;

        /// <summary>
        /// PI as a float (not a double)
        /// </summary>
        public const float PI_FLOAT = (float)Math.PI;

        /// <summary>
        /// Two times PI as a float (not a double)
        /// </summary>
        public const float TWO_PI_FLOAT = (float)Math.PI * 2;




        /// <summary>
        /// Calculate the sum of all consecutive integers between 0 and some value, inclusive.
        /// </summary>
        /// <param name="_int">The last consecutive integer to include in the sum</param>
        /// <returns>
        /// The sum of all integers between 0 and <paramref name="_int"/> , inclusive
        /// </returns>
        public static int SumOfConsecutiveInts( int _int ) => _int * (_int + 1) / 2;

        /// <summary>
        /// Calculate the largest integer where the <see cref="SumOfConsecutiveInts"/> is less
        /// than or equal to some value.
        /// </summary>
        /// <param name="_sum">An integer</param>
        /// <returns>
        /// The largest integer producing a "SumOfConsecutiveInts" less than or equal to
        /// <paramref name="_sum"/> .
        /// </returns>
        public static int InverseSumOfConsecutiveInts( int _sum ) => (int)((Math.Sqrt( 8 * _sum + 1 ) - 1) / 2);

        /// <summary>
        /// A sigmoid function that will re-scale values between 0 and 1 to the curve between
        /// _base and (1 - _base).
        /// </summary>
        /// <param name="_x">The value to scale</param>
        /// <param name="_base">
        /// The return value for an input of 0, or (1-return) for an input of 1
        /// </param>
        /// <returns>
        /// Approaches 0 as _x approaches negative-infinity, and approches 1 as _x approaches
        /// infinity
        /// </returns>
        public static double Sigmoid( double _x, double _base )
        {
            var K = 2 * Math.Log( 1 / _base - 1 );
            var a = 1 + Math.Exp( -K * (_x - .5) );
            return 1 / a;
        }

        /// <summary>
        /// Given a number between -1 and 1, assumed to be a linear scale, translate the number
        /// onto an "S" with the degree being the "verticalness" of the middle line of the "S"
        /// </summary>
        /// <param name="_number"></param>
        /// <param name="_degree"></param>
        /// <returns></returns>
        public static double CheapSigmoid( this double _number, double _degree )
            => Math.Sign( _number ) * Math.Pow( Math.Abs( _number ), 1.0 / _degree );

        /// <summary>
        /// Given a number between 0 and 1, assumed to be a linear scale, translate the number
        /// onto an "S" with the degree being the "verticalness" of the middle line of the "S"
        /// </summary>
        /// <param name="_number"></param>
        /// <param name="_degree"></param>
        /// <returns></returns>
        public static double CheapSigmoidZeroBased( this double _number, double _degree )
        {
            var x = _number * 2 - 1;
            x = x.CheapSigmoid( _degree );
            return (x + 1) / 2.0;
        }

        /// <summary>
        /// Return a number that's guaranteed to be greater than min, but less than max, and
        /// equal to "this" number if its in this range.
        /// </summary>
        /// <param name="_value">The value to test</param>
        /// <param name="_min">If the number is less than this value, return this value</param>
        /// <param name="_max">
        /// If the number is greater than this value, return this value
        /// </param>
        /// <returns>A number "clamped" between two numbers</returns>
        public static double Clamp( this double _value, double _min, double _max )
        {
            if (_value < _min)
                return _min;
            if (_value > _max)
                return _max;
            return _value;
        }

        /// <summary>
        /// Return a number that's guaranteed to be greater than min, but less than max, and
        /// equal to "this" number if its in this range.
        /// </summary>
        /// <param name="_value">The value to test</param>
        /// <param name="_min">If the number is less than this value, return this value</param>
        /// <param name="_max">
        /// If the number is greater than this value, return this value
        /// </param>
        /// <returns>A number "clamped" between two numbers</returns>
        public static float Clamp( this float _value, float _min, float _max )
        {
            if (_value < _min)
                return _min;
            if (_value > _max)
                return _max;
            return _value;
        }

        /// <summary>
        /// Return a number that's guaranteed to be greater than or equal to min, but less than
        /// or equal to max, and equal to "this" number if its in this range.
        /// </summary>
        /// <param name="_value">The value to test</param>
        /// <param name="_min">If the number is less than this value, return this value</param>
        /// <param name="_max">
        /// If the number is greater than this value, return this value
        /// </param>
        /// <returns>A number "clamped" between two numbers</returns>
        public static int Clamp( this int _value, int _min, int _max )
        {
            if (_value < _min)
                return _min;
            if (_value > _max)
                return _max;
            return _value;
        }

        /// <summary>
        /// Order two numbers- Swap them if they are not in ascending order.
        /// </summary>
        /// <param name="_1">The first number, you want this to be smaller</param>
        /// <param name="_2">The second number, you want this to be larger</param>
        public static void Order( ref double _1, ref double _2 )
        {
            if (_1 > _2)
            {
                var tmp = _1;
                _1 = _2;
                _2 = tmp;
            }
        }

        /// <summary>
        /// Order two numbers- Swap them if they are not in ascending order.
        /// </summary>
        /// <param name="_1">The first number, you want this to be smaller</param>
        /// <param name="_2">The second number, you want this to be larger</param>
        public static void Order( ref float _1, ref float _2 )
        {
            if (_1 > _2)
            {
                var tmp = _1;
                _1 = _2;
                _2 = tmp;
            }
        }

        /// <summary>
        /// Order two numbers- Swap them if they are not in ascending order.
        /// </summary>
        /// <param name="_1">The first number, you want this to be smaller</param>
        /// <param name="_2">The second number, you want this to be larger</param>
        public static void Order( ref byte _1, ref byte _2 )
        {
            if (_1 > _2)
            {
                var tmp = _1;
                _1 = _2;
                _2 = tmp;
            }
        }

        /// <summary>
        /// Order two numbers- Swap them if they are not in ascending order.
        /// </summary>
        /// <param name="_1">The first number, you want this to be smaller</param>
        /// <param name="_2">The second number, you want this to be larger</param>
        public static void Order( ref short _1, ref short _2 )
        {
            if (_1 > _2)
            {
                var tmp = _1;
                _1 = _2;
                _2 = tmp;
            }
        }

        /// <summary>
        /// Order two numbers- Swap them if they are not in ascending order.
        /// </summary>
        /// <param name="_1">The first number, you want this to be smaller</param>
        /// <param name="_2">The second number, you want this to be larger</param>
        public static void Order( ref int _1, ref int _2 )
        {
            if (_1 > _2)
            {
                var tmp = _1;
                _1 = _2;
                _2 = tmp;
            }
        }

        /// <summary>
        /// Order two numbers- Swap them if they are not in ascending order.
        /// </summary>
        /// <param name="_1">The first number, you want this to be smaller</param>
        /// <param name="_2">The second number, you want this to be larger</param>
        public static void Order( ref long _1, ref long _2 )
        {
            if (_1 > _2)
            {
                var tmp = _1;
                _1 = _2;
                _2 = tmp;
            }
        }


        /// <summary>
        /// Determine if a value is "between" two other values- If the value EQUALS either
        /// value, it IS considered "between" them. The parameters can be in either order.
        /// </summary>
        /// <param name="_value">The value to test</param>
        /// <param name="_1">The first value to check against</param>
        /// <param name="_2">The second value to check against</param>
        /// <returns></returns>
        public static bool IsBetween( this double _value, double _1, double _2 )
            => (_value >= _1 && _value <= _2) || (_value >= _2 && _value <= _1);

        /// <summary>
        /// Determine if a value is "between" two other values- If the value EQUALS either
        /// value, it IS considered "between" them. The parameters can be in either order.
        /// </summary>
        /// <param name="_value">The value to test</param>
        /// <param name="_1">The first value to check against</param>
        /// <param name="_2">The second value to check against</param>
        /// <returns></returns>
        public static bool IsBetween( this float _value, float _1, float _2 )
            => (_value >= _1 && _value <= _2) || (_value >= _2 && _value <= _1);

        /// <summary>
        /// Determine if a value is "between" two other values- If the value EQUALS either
        /// value, it IS considered "between" them. The parameters can be in either order.
        /// </summary>
        /// <param name="_value">The value to test</param>
        /// <param name="_1">The first value to check against</param>
        /// <param name="_2">The second value to check against</param>
        /// <returns></returns>
        public static bool IsBetween( this byte _value, byte _1, byte _2 )
            => (_value >= _1 && _value <= _2) || (_value >= _2 && _value <= _1);

        /// <summary>
        /// Determine if a value is "between" two other values- If the value EQUALS either
        /// value, it IS considered "between" them. The parameters can be in either order.
        /// </summary>
        /// <param name="_value">The value to test</param>
        /// <param name="_1">The first value to check against</param>
        /// <param name="_2">The second value to check against</param>
        /// <returns></returns>
        public static bool IsBetween( this int _value, int _1, int _2 )
            => (_value >= _1 && _value <= _2) || (_value >= _2 && _value <= _1);

        /// <summary>
        /// Determine if a value is "between" two other values- If the value EQUALS either
        /// value, it IS considered "between" them. The parameters can be in either order.
        /// </summary>
        /// <param name="_value">The value to test</param>
        /// <param name="_1">The first value to check against</param>
        /// <param name="_2">The second value to check against</param>
        /// <returns></returns>
        public static bool IsBetween( this long _value, long _1, long _2 )
            => (_value >= _1 && _value <= _2) || (_value >= _2 && _value <= _1);




        /// <summary>
        /// Returns TRUE if two values are "close", as defined by one hundredth of one percent
        /// of the smaller of the two values
        /// </summary>
        /// <param name="_this">One of the values to test</param>
        /// <param name="_other">One of the values to test</param>
        /// <param name="_tolerance">
        /// How close the two values should be in order to be considered "Close"
        /// </param>
        /// <returns>TRUE if the two values are close to each other</returns>
        public static bool IsClose( this double _this, double _other, double _tolerance = 0.0001 )
        {
            var tolerance = Math.Min( _this, _other ) * _tolerance;

            var delta = _this - _other;
            if (delta < 0)
                delta = -delta;
            return delta < tolerance;
        }

        /// <summary>
        /// Returns TRUE if two values are "close", as defined by one hundredth of one percent
        /// of the smaller of the two values
        /// </summary>
        /// <param name="_this">One of the values to test</param>
        /// <param name="_other">One of the values to test</param>
        /// <returns>TRUE if the two values are close to each other</returns>
        public static bool IsClose( this float _this, float _other )
        {
            var tolerance = Math.Min( _this, _other ) * 0.0001f;

            var delta = _this - _other;
            if (delta < 0)
                delta = -delta;
            return delta < tolerance;
        }



        /// <summary>
        /// Make sure an angle, in radians, fits between ( -PI .. PI ]
        /// </summary>
        /// <param name="_radians">
        /// The angle to "fix" if its outside the range ( -PI .. PI ]
        /// </param>
        /// <returns>The angle, adjusted if necessary to fit within ( -PI .. PI ]</returns>
        public static double FixAngle( this double _radians )
        {
            var rad = _radians % TWO_PI; // yields a number between -2PI and 2PI

            if (rad > Math.PI)
                rad -= TWO_PI;
            else if (rad < -Math.PI)
                rad += TWO_PI;

            return rad;
        }

        /// <summary>
        /// Make sure an angle, in radians, fits between ( -PI .. PI ]
        /// </summary>
        /// <param name="_radians">
        /// The angle to "fix" if its outside the range ( -PI .. PI ]
        /// </param>
        /// <returns>The angle, adjusted if necessary to fit within ( -PI .. PI ]</returns>
        public static float FixAngle( this float _radians )
        {
            var rad = _radians % TWO_PI_FLOAT; // yields a number between -2PI and 2PI

            if (rad > PI_FLOAT)
                rad -= TWO_PI_FLOAT;
            else if (rad < -PI_FLOAT)
                rad += TWO_PI_FLOAT;

            return rad;
        }


        /// <summary>
        /// Linear Interpolation between two values- Lerp amount 0 returns _1, Lerp amount 1 =
        /// _2
        /// </summary>
        /// <param name="_1">The first point, corresponding to Lerp Amount == 0</param>
        /// <param name="_2">The second point, corresponding to Lerp Amount == 1</param>
        /// <param name="_lerpAmount">The position, relative to _1 and _2</param>
        /// <returns>
        /// A value linearly interpolated between _1 and _2 based on _lerpAmount
        /// </returns>
        public static float Lerp( float _1, float _2, float _lerpAmount )
        {
            var dist = _2 - _1;
            return _lerpAmount * dist + _1;
        }

        /// <summary>
        /// Linear Interpolation between two values- Lerp amount 0 returns _1, Lerp amount 1 =
        /// _2
        /// </summary>
        /// <param name="_1">The first point, corresponding to Lerp Amount == 0</param>
        /// <param name="_2">The second point, corresponding to Lerp Amount == 1</param>
        /// <param name="_lerpAmount">The position, relative to _1 and _2</param>
        /// <returns>
        /// A value linearly interpolated between _1 and _2 based on _lerpAmount
        /// </returns>
        public static double Lerp( double _1, double _2, double _lerpAmount )
        {
            var dist = _2 - _1;
            return _lerpAmount * dist + _1;
        }


        /// <summary>
        /// Solve a quadratic equation in the form ax^2 + bx + c = 0
        /// </summary>
        /// <param name="_a">Coefficient for x^2</param>
        /// <param name="_b">Coefficient for x</param>
        /// <param name="_c">Constant</param>
        /// <param name="_solution1">The first solution</param>
        /// <param name="_solution2">The second solution</param>
        /// <returns>TRUE if a solution exists, FALSE if one does not</returns>
        public static bool QuadraticSolver( double _a, double _b, double _c, out double _solution1, out double _solution2 )
        {
            if (_a == 0)
            {
                if (_b == 0)
                {
                    _solution1 = _solution2 = double.NaN;
                    return false;
                }
                else
                {
                    _solution1 = _solution2 = -_c / _b;
                    return true;
                }
            }

            var tmp = _b * _b - 4 * _a * _c;
            if (tmp < 0)
            {
                _solution1 = _solution2 = double.NaN;
                return false;
            }

            tmp = Math.Sqrt( tmp );
            var _2a = 2 * _a;
            _solution1 = (-_b + tmp) / _2a;
            _solution2 = (-_b - tmp) / _2a;
            return true;
        }

        /// <summary>
        /// This is a speed-optimized calculator of the integral value of the "log2" (logarithm
        /// base 2) of an Int32
        /// </summary>
        /// <param name="_number">The number to calculate the Log2 of</param>
        /// <returns>The integer part of the Log2 of _number</returns>
        public static int Log2Int( this int _number )
        {
            var bits = 0;
            var n = _number;

            if (n > 0xffff)
            {
                n >>= 16;
                bits = 0x10;
            }

            if (n > 0xff)
            {
                n >>= 8;
                bits |= 0x8;
            }

            if (n > 0xf)
            {
                n >>= 4;
                bits |= 0x4;
            }

            if (n > 0x3)
            {
                n >>= 2;
                bits |= 0x2;
            }

            if (n > 0x1)
            {
                bits |= 0x1;
            }

            // Note- conscious choice to return 0 if _number==0, even though the log is
            // undefined for 0.
            return bits;
        }

        /// <summary>
        /// Return the greatest common divisor of two integers.
        /// </summary>
        /// <param name="_x">One of the integers</param>
        /// <param name="_y">Another of the integers</param>
        /// <returns>The GCD of the two integers</returns>
        public static int GCD( int _x, int _y )
        {
            _x = Math.Abs( _x );
            _y = Math.Abs( _y );
            if (_x * _y == 0)
                return 0;

            for (int r; _y != 0; _y = r)
            {
                r = _x % _y;
                _x = _y;
            }

            return _x;
        }

        /// <summary>
        /// Return the greatest common divisor of two integers.
        /// </summary>
        /// <param name="_x">One of the integers</param>
        /// <param name="_y">Another of the integers</param>
        /// <returns>The GCD of the two integers</returns>
        public static long GCD( long _x, long _y )
        {
            _x = Math.Abs( _x );
            _y = Math.Abs( _y );
            if (_x * _y == 0)
                return 0;

            for (long r; _y != 0; _y = r)
            {
                r = _x % _y;
                _x = _y;
            }

            return _x;
        }

        /// <summary>
        /// Return the least common multiple of an array of integers. Can be used as a variadic
        /// function.
        /// </summary>
        /// <param name="_numbers">
        /// A set of numbers, either as an array or a parameter list
        /// </param>
        /// <returns>The LCM of the integers</returns>
        public static long LCM( params long[] _numbers )
        {
            long min, max, lcm = 0;

            for (var i = 0; i < _numbers.Length; i++)
            {
                for (var j = i + 1; j < _numbers.Length - 1; j++)
                {
                    if (_numbers[i] > _numbers[j])
                    {
                        min = _numbers[j];
                        max = _numbers[i];
                    }
                    else
                    {
                        min = _numbers[i];
                        max = _numbers[j];
                    }
                    for (var k = 0; k < _numbers.Length; k++)
                    {
                        var x = k * max;
                        if (x % min == 0)
                            lcm = x;
                    }
                }
            }

            return lcm;
        }

        /// <summary>
        /// Return the difference between this value and some target value, represented as a
        /// fraction of the target value.
        /// </summary>
        /// <param name="_val"></param>
        /// <param name="_target"></param>
        /// <returns></returns>
        public static double DifferenceAsRatioOf( this double _val, double _target ) => (_val - _target) / _target;







        /// <summary>
        /// This function converts an unsigned binary number to reflected binary Gray code.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static byte BinaryToGray( this byte num )
        {
            return (byte)(num ^ (num >> 1)); // The operator >> is shift right. The operator ^ is exclusive or.
        }

        /// <summary>
        /// This function converts an unsigned binary number to reflected binary Gray code.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ushort BinaryToGray( this ushort num )
        {
            return (ushort)(num ^ (num >> 1)); // The operator >> is shift right. The operator ^ is exclusive or.
        }

        /// <summary>
        /// This function converts an unsigned binary number to reflected binary Gray code.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static uint BinaryToGray( this uint num )
        {
            return num ^ (num >> 1); // The operator >> is shift right. The operator ^ is exclusive or.
        }

        /// <summary>
        /// This function converts an unsigned binary number to reflected binary Gray code.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ulong BinaryToGray( this ulong num )
        {
            return num ^ (num >> 1); // The operator >> is shift right. The operator ^ is exclusive or.
        }

        // This function converts a reflected binary Gray code number to a binary number in a
        // generalized manner
        //<code>
        //uint GrayToBinary( uint num )
        //{
        //    uint mask = num;
        //    while (mask)
        //    {           // Each Gray code bit is exclusive-ored with all more significant bits.
        //        mask >>= 1;
        //        num ^= mask;
        //    }
        //    return num;
        //}

        /// <summary>
        /// A more efficient version for Gray codes 32 bits or fewer through the use of SWAR
        /// (SIMD within a register) techniques. It implements a parallel prefix XOR function.
        /// The assignment statements can be in any order.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static byte GrayToBinary( this byte num )
        {
            num ^= (byte)(num >> 4);
            num ^= (byte)(num >> 2);
            num ^= (byte)(num >> 1);
            return num;
        }

        /// <summary>
        /// A more efficient version for Gray codes 32 bits or fewer through the use of SWAR
        /// (SIMD within a register) techniques. It implements a parallel prefix XOR function.
        /// The assignment statements can be in any order.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ushort GrayToBinary( this ushort num )
        {
            num ^= (byte)(num >> 8);
            num ^= (byte)(num >> 4);
            num ^= (byte)(num >> 2);
            num ^= (byte)(num >> 1);
            return num;
        }

        /// <summary>
        /// A more efficient version for Gray codes 32 bits or fewer through the use of SWAR
        /// (SIMD within a register) techniques. It implements a parallel prefix XOR function.
        /// The assignment statements can be in any order.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static uint GrayToBinary( this uint num )
        {
            num ^= (byte)(num >> 16);
            num ^= (byte)(num >> 8);
            num ^= (byte)(num >> 4);
            num ^= (byte)(num >> 2);
            num ^= (byte)(num >> 1);
            return num;
        }

        /// <summary>
        /// A more efficient version for Gray codes 32 bits or fewer through the use of SWAR
        /// (SIMD within a register) techniques. It implements a parallel prefix XOR function.
        /// The assignment statements can be in any order.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ulong GrayToBinary( this ulong num )
        {
            num ^= num >> 32;
            num ^= num >> 16;
            num ^= num >> 8;
            num ^= num >> 4;
            num ^= num >> 2;
            num ^= num >> 1;
            return num;
        }


    }
}
