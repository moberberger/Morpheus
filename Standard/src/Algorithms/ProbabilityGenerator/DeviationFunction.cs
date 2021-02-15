using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// Identifies a Deviation Function, along with its parameters
    /// </summary>
    public abstract class DeviationFunction
    {
        protected readonly int Version;
        protected DeviationFunction( int version ) => Version = version;

        /// <summary>
        /// Set the values in the subclass
        /// </summary>
        /// <param name="_in"></param>
        /// <param name="_out"></param>
        /// <param name="_detail"></param>
        public abstract Output CalculateDeviation( Input _in, Output _out, DeviationDetail _detail = null );

        /// <summary>
        /// Allows the DeviationFunction to create a deviation detail object of the correct type
        /// </summary>
        /// <returns></returns>
        public abstract DeviationDetail NewDeviationDetailObject();
    }

}
