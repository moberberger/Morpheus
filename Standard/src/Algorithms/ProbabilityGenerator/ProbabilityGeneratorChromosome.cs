using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// 
    /// </summary>
    public class ProbabilityGeneratorChromosome : Chromosome
    {
        /// <summary>
        /// The probabilities associated with the best deviation found by the algorithm
        /// 
        /// this and only this field must be set prior to calling the deviation function
        /// </summary>
        public readonly double[] Probabilities;

        /// <summary>
        /// The value associated with the best deviation found by the algorithm.
        /// </summary>
        public double CalculatedValue = double.MinValue;

        /// <summary>
        /// How many probabilities are there?
        /// </summary>
        public int ProbabilityCount => Probabilities.Length;

        /// <summary>
        /// Construct an Output Initialization Array with knowledge of the values associated
        /// with the probabilities
        /// </summary>
        /// <param name="config"></param>
        public ProbabilityGeneratorChromosome( ProbabilityGeneratorConfig config )
            : base( VersionInfo.ProbabilityGeneratorChromosome )
        {
            Probabilities = Lib.CreatePopulatedArray( config.ValueCount, () => Math.Abs( DI.Default.Get<Random>().NextGaussian( 0, 1 ) ) );
            Probabilities.ChangeToProbabilities();
        }

        /// <summary>
        /// Construct with the size of the probability array = input value array size
        /// </summary>
        /// <param name="other"></param>
        public ProbabilityGeneratorChromosome( Chromosome other )
            : base( VersionInfo.ProbabilityGeneratorChromosome )
            => CopyFrom( other );

        /// <summary>
        /// Construct an empty Output record
        /// </summary>
        /// <param name="count"></param>
        public ProbabilityGeneratorChromosome( int count )
            : base( VersionInfo.ProbabilityGeneratorChromosome )
            => Probabilities = new double[count];

        /// <summary>
        /// Copy the data from another Output object
        /// </summary>
        /// <param name="other"></param>
        public override void CopyFrom( Chromosome other )
        {
            base.CopyFrom( other );
            var chromo = other as ProbabilityGeneratorChromosome; // should work because of base method

            Array.Copy( chromo.Probabilities, Probabilities, ProbabilityCount );
            CalculatedValue = chromo.CalculatedValue;
        }

    }

}
