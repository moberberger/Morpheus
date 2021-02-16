using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// Evolve by mutating one or more values using a random (with normal distribution)
    /// multiple
    /// </summary>
    public class GeneticesqueAlgorithm : EvolutionAlgorithm
    {
        public GeneticesqueAlgorithm() : base( VersionInfo.GeneticesqueAlgorithm ) { }

        public virtual double MinimumProbability { get; set; } = 1e-20;

        public virtual int PopulationSize { get; set; } = 300;

        public virtual double MutationChance { get; set; } = 0.05;
        public virtual double MultiMutateChance { get; set; } = 0.30;

        public virtual double MinMutationStdev { get; set; } = 1.0;
        public virtual double MaxMutationStdev { get; set; } = 40.0;
        public virtual double MutationStdev
        { // be more exploratory when the deviation is high
            get
            {
                if (Best == null) return MaxMutationStdev;
                var err = Math.Sqrt( Best.Deviation );
                var retval = err.Clamp( MinMutationStdev, MaxMutationStdev );
                return retval;
            }
        }


        /// <summary>
        /// Generate the Output based on the Input and the parameters provided
        /// </summary>
        /// <param name="input"></param>
        /// <param name="deviationFn"></param>
        /// <returns></returns>
        public override Output Generate( Input input, DeviationFunction deviationFn )
        {
            var sampleSet = Lib.Repeat( PopulationSize, () => new Output( input ) )
                               .Select( _x => deviationFn.CalculateDeviation( input, _x ) )
                               .OrderBy( _x => _x.Deviation )
                               .ToList();

            var resultSet = new List<Output>();

            TerminateCalculation = false;
            var detail = deviationFn.NewDeviationDetailObject();

            foreach (var _ in Iterate())
            {
                resultSet.Add( sampleSet[0] ); // Elitism

                // Generate new outputs
                for (int i = 1; i < PopulationSize; i++)
                {
                    var result = new Output( input );
                    Evolve( () => sampleSet.Sample( _x => _x.Deviation, true ), result );
                    deviationFn.CalculateDeviation( input, result );
                    resultSet.Add( result );
                }

                // Process outputs into new SampleSet
                resultSet.Sort();
                deviationFn.CalculateDeviation( input, resultSet[0] );

                sampleSet.Clear();
                sampleSet.AddRange( resultSet.Take( PopulationSize ) );
                sampleSet.Sort();

                resultSet.Clear();


                Best = sampleSet[0]; // list was sorted, this is best

                deviationFn.CalculateDeviation( input, Best, detail );

                Console.WriteLine( $"[{IterationCount}] {Best}-{detail}" );
            }

            return Best;
        }





        private void Evolve( Func<Output> generator, Output result )
        {
            if (DI.Default.Get<Random>().NextDouble() < MutationChance)
            {
                var basis = generator();
                Array.Copy( basis.Probabilities, result.Probabilities, basis.ProbabilityCount );

                do
                {
                    int idx = Rng.Default.Next( basis.ProbabilityCount );

                    var factor = Math.Abs( (double)Rng.Default.NextGaussian( 1, MutationStdev ) );

                    var newVal = basis.Probabilities[idx] * factor;

                    newVal = Math.Max( newVal, MinimumProbability );

                    result.Probabilities[idx] = newVal;

                } while (Rng.Default.NextDouble() < MultiMutateChance);

            }
            else
            {
                int len = result.ProbabilityCount;

                var p1 = generator();
                var p2 = generator();
                var idx1 = DI.Default.Get<Random>().Next( len );
                var idx2 = DI.Default.Get<Random>().Next( len );
                for (int i = 0; i < len; i++)
                {
                    if (i.IsBetween( idx1, idx2 ))
                        result.Probabilities[i] = p1.Probabilities[i];
                    else
                        result.Probabilities[i] = p2.Probabilities[i];
                }
            }

            result.Probabilities.ChangeToProbabilities();
        }





    }
}
