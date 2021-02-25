using System;
using System.Collections.Generic;
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
    public class LCPRNG_MMIX : LCPRNG
    {
        /// <summary>
        /// Create one. Don't worry about it.
        /// </summary>
        public LCPRNG_MMIX() : base( 6364136223846793005, 1442695040888963407 ) { }
    }
}
