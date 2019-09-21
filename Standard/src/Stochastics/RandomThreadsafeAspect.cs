using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// A Threadsafe wrapper around a <see cref="Random"/> or a <see cref="RandomAspect"/>
    /// object. Not doc'ed because, well, look at this trivia
    /// </summary>
    internal class RandomThreadsafeAspect : RandomAspect
    {
        private readonly object m_lock = new object();

        /// <summary>
        /// The RNG that is being wrapped
        /// </summary>
        internal readonly Random m_rng;

        /// <summary>
        /// Must construct with an RNG
        /// </summary>
        /// <param name="_rng">The RNG to be wrapped</param>
        internal RandomThreadsafeAspect( Random _rng ) => m_rng = _rng ?? throw new ArgumentNullException();


        /// <summary>
        /// This is the only function from the wrapped object used directly. 
        /// </summary>
        /// <param name="_buffer">The buffer to fill with random bytes</param>
        public override void NextBytes( byte[] _buffer )
        {
            lock (m_lock) m_rng.NextBytes( _buffer );
        }
    }
}
