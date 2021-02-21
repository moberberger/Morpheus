using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Morpheus
{
    public class LCPRNG_MMIX : LCPRNG
    {
        public LCPRNG_MMIX() : base( 6364136223846793005, 1442695040888963407 ) { }
    }

    /// <summary>
    /// Abstract base, needing only "a" and "c" a/c (multiplier and increment)
    /// 
    /// Maybe refactor to make non-abstract with a constructor- but bad a/c means really bad
    /// mismatch to expectations. So for now I'm forcing a level of purpose when setting a/c
    /// </summary>
    /// <remarks>
    /// Yes, technically should be called "ACPRNG". there's the acknowledgement.
    /// </remarks>
    public abstract class LCPRNG
    {
        private static int sm_instanceCount = 3;
        private ulong _multiplier;
        private ulong _increment;
        private ulong _state;


        protected LCPRNG( ulong multiplier, ulong increment )
        {
            _multiplier = multiplier;
            _increment = increment;

            // allows initialization to be multi-threaded
            var mult = Interlocked.Increment( ref sm_instanceCount );
            _state = (uint)DateTime.Now.Ticks;
            _state *= (ulong)mult;
        }

        /// <summary>
        /// No bias
        /// </summary>
        /// <returns>An unbiased PRNG value</returns>
        public int NextInt()
        {
            _state = _multiplier * _state + _increment;
            return (int)(_state & 0x7fff_ffff);
        }

        /// <summary>
        /// No bias
        /// </summary>
        /// <returns>An unbiased PRNG value</returns>
        public long NextLong()
        {
            _state = _multiplier * _state + _increment;
            return (long)(_state & 0x7fff_ffff_ffff_ffff);
        }

        /// <summary>
        /// No bias
        /// </summary>
        /// <returns>An unbiased PRNG value</returns>
        public uint NextUInt()
        {
            _state = _multiplier * _state + _increment;
            return (uint)(_state & 0xffff_ffff);
        }

        /// <summary>
        /// No bias
        /// </summary>
        /// <returns>An unbiased PRNG value</returns>
        public ulong NextULong()
        {
            return _state = _multiplier * _state + _increment;
        }

        /// <summary>
        /// Advance the PRNG state using data external to the PRNG. Much (3x+?) slower than
        /// <see cref="NextULong"/> , so should not be used as a substitute. If you need better
        /// randomness, use a differnet class- see <see cref="RandomAspect"/>
        /// </summary>
        /// <returns>A biased PRNG value</returns>
        public ulong Advance()
        {
            var factor = (ulong)Interlocked.Increment( ref sm_instanceCount );
            factor += (ulong)DateTime.Now.Ticks;
            var mask = _multiplier * factor + _increment;
            _state ^= mask;
            return _state;
        }

        /// <summary>
        /// An unbiased <see cref="double"/> value in [0..1) with 32 bits of precision. If more
        /// precision is needed, maybe this isn't the PRNG for you. See <see cref="RandomAspect"/>
        /// </summary>
        /// <returns></returns>
        public double NextDouble() => (double)NextUInt() / (1.0 + uint.MaxValue);

        /// <summary>
        /// Return a PRN scaled to a specified value.
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
        public int Next( int maxPlusOne ) => (int)(NextULong() % (ulong)maxPlusOne);


        public int Next( int min, int maxPlusOne ) => min + Next( maxPlusOne );
    }
}
