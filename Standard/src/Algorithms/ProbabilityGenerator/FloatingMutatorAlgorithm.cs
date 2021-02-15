using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// Evolve by mutating one or more values using a random (with normal distribution)
    /// multiple
    /// </summary>
    public class FloatingMutatorAlgorithm : EvolutionAlgorithm
    {
        public FloatingMutatorAlgorithm() : base( VersionInfo.FloatingMutatorAlgorithm ) { }

        public virtual double MinimumProbability { get; set; } = 1e-20;

        public virtual int PopulationSize { get; set; } = 300;

        public virtual int MutationCount { get; set; } = 2;

        public virtual double MultiMutateChance { get; set; } = 0.35;

        public virtual double MeanIncrementRate { get; set; } = 3;
        public virtual double MinStddevIncrementRate { get; set; } = 1.0;
        public virtual double MaxStddevIncrementRate { get; set; } = 20.0;
        public virtual double StddevIncrementRate
        { // be more exploratory when the deviation is high
            get
            {
                if (Best == null) return MaxStddevIncrementRate;
                var err = Math.Sqrt( Best.Deviation );
                var retval = err.Clamp( MinStddevIncrementRate, MaxStddevIncrementRate );
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

            foreach (var _ in Iterate())
            {
                resultSet.Add( sampleSet[0] ); // Elitism

                // Generate new outputs
                for (int i = 0; i < PopulationSize; i++)
                {
                    var obj = sampleSet.Sample( _x => _x.Deviation, true );

                    for (int j = 0; j < MutationCount; j++)
                    {
                        var result = new Output( obj );
                        Mutate( obj, result );
                        deviationFn.CalculateDeviation( input, result );
                        resultSet.Add( result );
                    }
                }

                // Process outputs into new SampleSet
                resultSet.Sort();
                sampleSet.Clear();
                sampleSet.AddRange( resultSet.Take( PopulationSize ) );

                resultSet.Clear();

                Best = sampleSet[0]; // list was sorted, this is best

                var detail = deviationFn.NewDeviationDetailObject();
                deviationFn.CalculateDeviation( input, Best, detail );

                Console.WriteLine( $"[{IterationCount}] {Best} {detail}" );
            }

            return Best;
        }





        private void Mutate( Output _in, Output _out )
        {
            Array.Copy( _in.Probabilities, _out.Probabilities, _in.ProbabilityCount );

            do
            {
                int idx = Rng.Default.Next( _in.ProbabilityCount );

                var factor = Math.Abs( (double)Rng.Default.NextGaussian( MeanIncrementRate, StddevIncrementRate ) );

                var newVal = _in.Probabilities[idx] * factor;

                newVal = Math.Max( newVal, MinimumProbability );

                _out.Probabilities[idx] = newVal;

            } while (Rng.Default.NextDouble() < MultiMutateChance);
        }





    }
}
