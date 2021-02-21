using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// This set of input values represents what every generator must accept, at a minimum.
    /// Subclasses may add further information to the algorithm input.
    /// 
    /// This data is transcendent across all deviation functions.
    /// </summary>
    public abstract class Config
    {
        /// <summary>
        /// Allows C/Asm to effect polymorphism
        /// </summary>
        protected readonly int Version = 0;

        /// <summary>
        /// Must be constructed with a version information value
        /// </summary>
        /// <param name="version"></param>
        protected Config( int version ) => Version = version;

        /// <summary>
        /// Create an "empty" chromosome- assumed to be quickest way to create a ready-to-use
        /// chromosome.
        /// </summary>
        /// <returns>
        /// A new chromosome object with no initialized data and NOT ready for use
        /// </returns>
        public abstract Chromosome CreateEmpty();

        /// <summary>
        /// Create a properly initialized (likely randomized) chromosome
        /// </summary>
        /// <returns>
        /// A new chromosome object with its values randomized and ready for use
        /// </returns>
        public abstract Chromosome CreateInitialized();
    }
}
