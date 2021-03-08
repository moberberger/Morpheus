using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// Knuth's MMIX full 64-bit LCPRNG.
    /// 
    /// No representation to its spectral characteristics.
    /// </summary>
    public class LCPRNG_MMIX : Rng
    {
        public const ulong Multiplier = 6364136223846793005UL;
        public const ulong Increment = 1442695040888963407UL;
        public ulong State = RandomSeed.FastULong();

        public override ulong Next64() => State = State * Multiplier + Increment;

        public static ulong Next( ulong state ) => state * Multiplier + Increment;
    }
}
