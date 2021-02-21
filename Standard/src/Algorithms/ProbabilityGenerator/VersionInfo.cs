using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.ProbabilityGeneratorNS
{
    public class VersionInfo
    {
        // Evolvers
        public const int FloatingMutatorEvolver = 0x0_0001;
        public const int GeneticesqueFloatEvolver = 0x0_0002;

        // Configurations
        public const int ProbabilityGeneratorConfig = 0x1_0001;
        public const int BitwiseChromosomeConfig = 0x1_0002;

        // Chromosomes
        public const int ProbabilityGeneratorChromosome = 0x2_0003;


        // Deviation Functions
        public const int GeneralizedDeviationFunction = 0x4_0001;
        public const int BalanceValueDeviationFunction = 0x4_0002;
        public const int BalanceProbabilityDeviationFunction = 0x4_0004;

        // Deviation Details
        public const int GeneralizedDeviationDetail = 0x8_0001;
        public const int BalanceDeviationDetail = 0x8_0002;
    }
}
