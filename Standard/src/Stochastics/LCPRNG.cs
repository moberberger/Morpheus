using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// Abstract base, needing only "a" and "c" a/c (multiplier and increment)
    /// 
    /// Maybe refactor to make non-abstract with a constructor- but bad a/c means really bad
    /// mismatch to expectations. So for now I'm forcing a level of purpose when setting a/c
    /// </summary>
    /// <remarks>
    /// Yes, technically should be called "ACPRNG". there's the acknowledgement.
    /// 
    /// When "bias" is mentioned in the comments in this class without clarification, I am
    /// referring to "additional bias added by this method's implementation". I am emphatically
    /// NOT referring to any bias inherent in LCPRNGs or in the coefficients used- that is a
    /// "different bias".
    /// </remarks>
    public abstract class LCPRNG : Random
    {
        /// <summary>
        /// Allows 52 bits to be masked off. Useful to create <see cref="double"/> values, which
        /// have 52 bits of precision.
        /// </summary>
        private const long DOUBLE_MASK = 0xf_ffff_ffff_ffff;

        /// <summary>
        /// The "a" coefficient
        /// </summary>
        private ulong _multiplier;

        /// <summary>
        /// The "c" coefficient
        /// </summary>
        private ulong _increment;

        /// <summary>
        /// The current state of the LCPRNG- also the last value returned by
        /// <see cref="Next64"/>
        /// </summary>
        private ulong _state;

        /// <summary>
        /// Maybe consider removing the "abstract" and making this public
        /// </summary>
        /// <param name="multiplier"></param>
        /// <param name="increment"></param>
        protected LCPRNG( ulong multiplier, ulong increment )
        {
            _multiplier = multiplier;
            _increment = increment;
            _state = RandomSeed.FastULong();
        }

        /// <summary>
        /// Advance the PRNG state using data external to the PRNG. Much (3x+?) slower than
        /// <see cref="Next64"/> , so should not be used as a substitute. If you need better
        /// randomness, use a differnet class- see <see cref="RandomAspect"/>
        /// </summary>
        /// <remarks>
        /// it is entirely possible that this method is entirely useless in real life.
        /// </remarks>
        /// <returns>A biased PRNG value</returns>
        public ulong Advance()
        {
            var mask = RandomSeed.FastULong();
            _state ^= mask;
            return _state;
        }

        /// <summary>
        /// No bias- This is the core generation function for this <see cref="Random"/>
        /// implementation
        /// </summary>
        /// <returns>An unbiased PRNG value</returns>
        public ulong Next64()
        {
            // Race Condition- Don't use this class in a re-entrant manner if you need stable
            // (repeatable) results

            var x = _state;
            x *= _multiplier;
            x += _increment;
            return _state = x;

            // End Race Condition
        }


        /// <summary>
        /// Implements <see cref="Random.Next()"/> without the bias introduced by the MS
        /// implementation of <see cref="Random"/>
        /// </summary>
        /// <returns></returns>
        public override int Next()
        {
            const int mask = 0x7fff_ffff;
            int retval;

            // eliminate bias
            for (retval = mask;
                 retval == mask;
                 retval = (int)(Next64() & 0x7fff_ffff))
                ;

            return retval;
        }

        /// <summary>
        /// Return a PRN scaled to a specified value in the range [0..maxPlusOne)
        /// </summary>
        /// <remarks>
        /// Naive in favor of speed.
        /// 
        /// Bias is spread over 64 bits, so a small-ish (~10bits) maxPlusOne will show bias of
        /// around 1/2^54. This is just fine for non-crypo non-statistically-sound output values
        /// </remarks>
        /// <param name="maxPlusOne">
        /// For numbers from 0 to 4 inclusive, set maxPlusOne to 5
        /// </param>
        /// <returns></returns>
        public override int Next( int maxPlusOne ) => (int)(Next64() % (ulong)maxPlusOne);

        /// <summary>
        /// Return a PRN in the range [min...maxPlusOne) with 32 bits of precision
        /// </summary>
        /// <param name="min"></param>
        /// <param name="maxPlusOne"></param>
        /// <returns></returns>
        public override int Next( int min, int maxPlusOne ) => min + Next( maxPlusOne - min );

        /// <summary>
        /// An unbiased <see cref="double"/> value in [0..1) with 32 bits of precision. If more
        /// precision is needed, maybe this isn't the PRNG for you. See
        /// <see cref="RandomAspect"/>
        /// </summary>
        /// <returns></returns>
        public override double NextDouble() => (Next64() & DOUBLE_MASK) / (1.0 + DOUBLE_MASK);



        /// <summary>
        /// Internal better generator
        /// </summary>
        /// <returns></returns>
        protected override double Sample() => NextDouble();



        /// <summary>
        /// No bias
        /// </summary>
        /// <returns>An unbiased PRNG value</returns>
        public uint Next32() => (uint)(Next64() & 0xffff_ffff);

        /// <summary>
        /// Biased- see <see cref="Next(int)"/>
        /// </summary>
        /// <returns>An unbiased PRNG value</returns>
        public long NextLong( long maxPlusOne ) => (long)(Next64() % (ulong)maxPlusOne);

        /// <summary>
        /// Return a PRN in the range [min...maxPlusOne) with 32 bits of precision
        /// </summary>
        /// <param name="min"></param>
        /// <param name="maxPlusOne"></param>
        /// <returns></returns>
        public long NextLong( long min, long maxPlusOne ) => min + NextLong( maxPlusOne - min );
    }
}
