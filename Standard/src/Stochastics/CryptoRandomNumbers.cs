using System;
using System.Security.Cryptography;

namespace Morpheus
{
    /// <summary>
    /// Use <see cref="RandomNumberGenerator"/> to generate random numbers using the
    /// <see cref="Random"/> usage model.
    /// </summary>
    public class CryptoRandomNumbers : RandomAspect
    {
        /// <summary>
        /// The rng itself. This does initialize itself using very reasonable algorithms, but
        /// not necessarily the best.
        /// </summary>
        private readonly RandomNumberGenerator m_rng = RandomNumberGenerator.Create();

        /// <summary>
        /// Fill the buffer with random bytes
        /// </summary>
        /// <param name="_buffer">The buffer to fill with random bytes</param>
        public override void NextBytes( byte[] _buffer ) => m_rng.GetBytes( _buffer );

        /// <summary>
        /// This class does have something to dispose
        /// </summary>
        public override void Dispose()
        {
            m_rng.Dispose();
            base.Dispose();
        }
    }
}
