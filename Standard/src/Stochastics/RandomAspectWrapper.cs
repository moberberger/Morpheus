using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// Internal class used to provide good implementations for scaling based on anyone's
    /// <see cref="Random"/> implementation, even the default.
    /// </summary>
    internal class RandomAspectWrapper : RandomAspect
    {


        /// <summary>
        /// The RNG that is being wrapped
        /// </summary>
        internal readonly Random m_rng;

        /// <summary>
        /// Must construct with an RNG
        /// </summary>
        /// <param name="_rng">The RNG to be wrapped</param>
        internal RandomAspectWrapper( Random _rng ) => m_rng = _rng ?? throw new ArgumentNullException();


        /// <summary>
        /// This is the only function from the wrapped object used directly. 
        /// </summary>
        /// <param name="_buffer">The buffer to fill with random bytes</param>
        public override void NextBytes( byte[] _buffer ) => m_rng.NextBytes( _buffer );
    }
}
