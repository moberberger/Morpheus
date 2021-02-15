using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// Identifies an Evolutionary Algorithm along with its parameters
    /// </summary>
    public abstract class EvolutionAlgorithm
    {
        protected readonly int Version;
        protected EvolutionAlgorithm( int version ) => Version = version;



        public int IterationCount = 0;
        public double DeviationTolerance = .40;
        public bool TerminateCalculation = false;

        public Output Best { get; internal set; }

        protected IEnumerable<int> Iterate()
        {
            for (IterationCount = 0; (Best == null || Best.Deviation > DeviationTolerance) && !TerminateCalculation; IterationCount++)
                yield return IterationCount;
        }

        public abstract Output Generate( Input _in, DeviationFunction _deviation );
    }

}
