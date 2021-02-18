using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// Generated after the Deviation Function evaluates an input/output set
    /// </summary>
    public class DeviationDetail
    {
        protected readonly int Version = VersionInfo.GeneralizedDeviationDetail;
        protected DeviationDetail( int version ) => Version = version;

        /// <summary>
        /// The overall deviation
        /// </summary>
        public double Deviation;
    }
}
