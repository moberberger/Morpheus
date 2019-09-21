using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// Class to provide default RNGs.
    /// </summary>
    public static class Rng
    {
        /// <summary>
        /// The Default Random Number Generator from Morpheus
        /// </summary>
        public static readonly RandomAspect Default = new CryptoRandomNumbers().Threadsafe();
    }
}
