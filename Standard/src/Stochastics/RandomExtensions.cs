using System;

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
        public static double NextGaussian( this Random _rng, double _mean = 0.0, double _standardDeviation = 1.0 ) =>
            NextGaussian( _rng ) * _standardDeviation + _mean;

        /// <summary>
        /// Returns a random floating-point (double) value that represents a Normal distribution
        /// about zero with a standard deviation of 1
        /// </summary>
        /// <param name="_rng"></param>
        /// <returns></returns>
        public static double NextGaussian( this Random _rng )
        {
            double r1, r2, rsq;

            do
            {
                r1 = _rng.NextDouble() * 2 - 1;
                r2 = _rng.NextDouble() * 2 - 1;

                // rsq will represent the square of the magnitude of (r1, r2)
                rsq = r1 * r1 + r2 * r2;
            }
            while (rsq >= 1.0 || rsq == 0.0); // outside unit circle || at origin

            var fac = Math.Sqrt( -2.0 * Math.Log( rsq ) / rsq );

            // either r1 or r2 works here- arbitrary choice
            return r1 * fac;
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
            // Each "Window" for "frame" contains exactly _exclusiveMax integers
            var windowCount = _maxValue / _exclusiveMax;
            var maxOKValue = windowCount * _exclusiveMax;

            // If the random bits are small enough to provide a full "window", then the value is
            // unbiased. If it is not, then it falls into that last little partial "window"
            // between [maxOkValue.._maxValue]
            _isUnbiased = _value < maxOKValue;

            // This is the same regardless of the presence of bias
            return _value % _exclusiveMax;
        }

        #endregion
    }
}
