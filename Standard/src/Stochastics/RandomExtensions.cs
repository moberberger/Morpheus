using System;
using System.Net.Mail;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// The collection of methods provided by Morpheus on top of any <see cref="Random"/>
    /// implementation.
    /// </summary>
    public static class RandomExtensions
    {
        #region Gaussian

        /// <summary>
        /// Returns a random floating-point (double) value that represents a Normal distribution
        /// around a specific mean with a specified value for standard deviation.
        /// </summary>
        /// <param name="_rng">The RNG supplying randomness</param>
        /// <param name="_mean">The mean value for the normal distribution</param>
        /// <param name="_standardDeviation">
        /// The standard deviation governing the shape of the normal distribution curve
        /// </param>
        /// <returns>
        /// A random floating-point (double) value that represents a Normal distribution around
        /// _mean with a standard deviation of _standardDeviation
        /// </returns>
        /// <remarks>
        /// <para> Finds a random point (r1,r2) that is within the unit circle, excluding the
        /// origin. See pp. 312-314 in "Numerical Recipes in C (2nd Ed.)" entitled "Normal
        /// (Gaussian) Deviates". </para>
        /// <para> Roughly 68.269% of values will be +/- ONE _standardDeviation of _mean </para>
        /// <para> Roughly 95.450% of values will be +/- TWO _standardDeviations of _mean </para>
        /// <para> Roughly 99.730% of values will be +/- THREE _standardDeviations of _mean </para>
        /// <para> Roughly 99.994% of values will be +/- FOUR _standardDeviations of _mean </para>
        /// </remarks>
        public static double NextGaussian( this Random _rng, double _mean = 0.0, double _standardDeviation = 1.0 )
        {
            double r1, r2, rsq, fac;

            do
            {
                r1 = _rng.NextDouble() * 2 - 1;
                r2 = _rng.NextDouble() * 2 - 1;

                // rsq will represent the square of the magnitude of (r1, r2)
                rsq = r1 * r1 + r2 * r2;
            }
            while (rsq >= 1.0 || rsq == 0.0);

            fac = Math.Sqrt( -2.0 * Math.Log( rsq ) / rsq );

            // either r1 or r2 works here- arbitrary choice
            return r1 * fac * _standardDeviation + _mean;
        }

        #endregion


        #region Dice

        /// <summary>
        /// Simulate the rolling of a number of dice, where each die has the same number of
        /// sides.
        /// </summary>
        /// <param name="_rng">The Random Number Generator</param>
        /// <param name="_numberOfDice">The number of dice to roll.</param>
        /// <param name="_sidesPerDie">The number of sides on each die rolled.</param>
        /// <returns>The sum of the results of all of the dice</returns>
        public static int RollDice( this Random _rng, int _numberOfDice, int _sidesPerDie )
        {
            if (_numberOfDice < 1)
                throw new ArgumentException( "Number of Dice needs to be 1 or greater" );
            if (_sidesPerDie < 1)
                throw new ArgumentException( "Sides per Die needs to be 1 or greater" );

            var sum = _numberOfDice; // makes sure is 1-N, not 0-(N-1)
            for (var i = 0; i < _numberOfDice; i++)
                sum += _rng.Next( _sidesPerDie );

            return sum;
        }

        #endregion


        #region Scaling

        /// <summary>
        /// A modulo operation that detects what would be considered "bias" when scaling a
        /// random number from one range to another.
        /// </summary>
        /// <param name="_value">The generated random number</param>
        /// <param name="_exclusiveMax">returned value will be in [0.._exclusiveMax)</param>
        /// <param name="_maxValue">
        /// What should be considered the inclusive "Max Value" for the _value parameter. Two
        /// possible reasons for providing this value (and not accepting the default value of
        /// <see cref="ulong.MaxValue"/> ) is (1) Unit Testing; and (2) Scaling a 32-bit number
        /// instead of a 64-bit number
        /// </param>
        /// <param name="_isUnbiased">
        /// When TRUE, there is no bias in the return value. FALSE indicates that you should
        /// probably generate a new random number.
        /// </param>
        /// <returns>The modulo operation (_value % _exclusiveMax)</returns>
        /// <remarks>
        /// <code>
        /// ulong x = 0xffff_ffff_ffff_fff4;
        /// ulong max = 37;
        /// 
        /// ulong scaled = x.ScaleValue( max, ulong.MaxValue, out bool isValid );
        /// Assert.IsFalse( isValid );
        /// 
        /// var expected = x % max;
        /// Assert.AreEqual( scaled, expected );
        /// </code>
        /// </remarks>
        public static ulong ScaleValue( this ulong _value, ulong _exclusiveMax, ulong _maxValue, out bool _isUnbiased )
        {
            // Each "Window" contains exactly _exclusiveMax integers
            var windowCount = _maxValue / _exclusiveMax;
            var maxOKValue = windowCount * _exclusiveMax;

            // If the random bits are small enough to provide a full "window", then the value is
            // unbiased. If it is not, then it falls into that last little partial "window"
            // between [maxOkValue.._maxValue]
            _isUnbiased = _value < maxOKValue;

            // This is the same regardless of the presence of bias
            return _value % _exclusiveMax;
        }


        private static readonly ThreadLocal<byte[]> sm_scaleBuffer4 = new ThreadLocal<byte[]>( () => new byte[4] );
        private static readonly ThreadLocal<byte[]> sm_scaleBuffer8 = new ThreadLocal<byte[]>( () => new byte[8] );

        /// <summary>
        /// Continuously pull <see cref="int"/> values from the provided RNG and scale them
        /// until an unbiased value is returned.
        /// </summary>
        /// <param name="_rng">Source of randomness</param>
        /// <param name="_exclusiveMax">
        /// One greater than the maximum return value of this method.
        /// </param>
        /// <returns>An unbiased value in [0.._exclusiveMax)</returns>
        public static uint ScaleValue( this Random _rng, uint _exclusiveMax )
        {
            while (true)
            {
                uint rn;

                if (_rng is RandomAspect _rngAspect)
                {
                    rn = _rngAspect.Next32();
                }
                else
                {
                    var buffer = sm_scaleBuffer4.Value;
                    _rng.NextBytes( buffer );
                    rn = BitConverter.ToUInt32( buffer, 0 );
                }

                var retval = ScaleValue( rn, (ulong)_exclusiveMax, uint.MaxValue, out var isUnbiased );
                if (isUnbiased)
                    return (uint)retval;
            }
        }

        /// <summary>
        /// Continuously pull <see cref="System.Int32"/> values from the provided RNG and scale
        /// them until an unbiased value is returned.
        /// </summary>
        /// <param name="_rng">Source of randomness</param>
        /// <param name="_exclusiveMax">
        /// One greater than the maximum return value of this method.
        /// </param>
        /// <returns>An unbiased value in [0.._exclusiveMax)</returns>
        public static ulong ScaleValue( this Random _rng, ulong _exclusiveMax )
        {
            while (true)
            {
                ulong rn;

                if (_rng is RandomAspect _rngAspect)
                {
                    rn = _rngAspect.Next64();
                }
                else
                {
                    var buffer = sm_scaleBuffer8.Value;
                    _rng.NextBytes( buffer );
                    rn = BitConverter.ToUInt64( buffer, 0 );
                }

                var retval = ScaleValue( rn, (ulong)_exclusiveMax, ulong.MaxValue, out var isUnbiased );
                if (isUnbiased)
                    return (ulong)retval;
            }
        }

        #endregion


        #region Buffer Filling

        /// <summary>
        /// Convert a series of integers to a byte array
        /// </summary>
        /// <param name="_array"></param>
        /// <param name="_generator"></param>
        /// <remarks>
        /// <code>
        /// uint counter = 1;
        /// byte[] array = new byte[11];
        /// 
        /// array.FromIntegers( () => counter++ );
        /// 
        /// byte[] expected = new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0 };
        /// 
        /// array.Collate( expected, ( x, y ) => Assert.AreEqual( x, y ) );
        /// </code>
        /// </remarks>
        public static void FromIntegers( this byte[] _array, Func<uint> _generator )
        {
            const int wordSize = 4;

            // Setup loop that will simultaneously iterate over the output buffer and 64-bit
            // values coming from the RNG
            var wordsRemaining = 0; // assure RNG is called on 1st iteration of loop
            uint rngVal = 0;

            // Keep adding to the output buffer until we've satisfied the request
            for (var i = 0; i < _array.Length; i++)
            {
                // If there are no bytes left in the "rngVal", then get more bytes!
                if (wordsRemaining < 1)
                {
                    rngVal = _generator();
                    wordsRemaining = wordSize;
                }

                // Strip off low order 8 bits
                _array[i] = (byte)(rngVal & 0xff);

                // Move the next 8 bits into position
                rngVal >>= 8;

                // Loop control
                wordsRemaining--;
            }
        }

        /// <summary>
        /// Convert a series of integers to a byte array
        /// </summary>
        /// <param name="_array"></param>
        /// <param name="_generator"></param>
        /// <remarks>
        /// <code>
        /// uint counter = 1;
        /// byte[] array = new byte[11];
        /// 
        /// array.FromIntegers( () => counter++ );
        /// 
        /// byte[] expected = new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0 };
        /// 
        /// array.Collate( expected, ( x, y ) => Assert.AreEqual( x, y ) );
        /// </code>
        /// </remarks>
        public static void FromIntegers( this byte[] _array, Func<ulong> _generator )
        {
            const int wordSize = 8;

            // Setup loop that will simultaneously iterate over the output buffer and 64-bit
            // values coming from the RNG
            var bytesRemainingInWord = 0; // assure RNG is called on 1st iteration of loop
            ulong rngVal = 0;

            // Keep adding to the output buffer until we've satisfied the request
            for (var i = 0; i < _array.Length; i++)
            {
                // If there are no bytes left in the "rngVal", then get more bytes!
                if (bytesRemainingInWord < 1)
                {
                    rngVal = _generator();
                    bytesRemainingInWord = wordSize;
                }

                // Strip off low order 8 bits
                _array[i] = (byte)(rngVal & 0xff);

                // Move the next 8 bits into position
                rngVal >>= 8;

                bytesRemainingInWord--;
            }
        }

        #endregion



        /// <summary>
        /// Return a fully featured threadsafe <see cref="RandomAspect"/> from any given
        /// <see cref="Random"/> object.
        /// </summary>
        /// <param name="_rng">The <see cref="Random"/> object to wrap</param>
        /// <returns>A fully featured threadsafe <see cref="RandomAspect"/></returns>
        public static RandomAspect Threadsafe( this Random _rng )
        {
            switch (_rng)
            {
                case RandomThreadsafeAspect rtsa:
                    return rtsa;
                default:
                    return new RandomThreadsafeAspect( _rng );
            }
        }

    }
}
