using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Morpheus.Evolution.PGNS
{
    /// <summary>
    /// Evolve by mutating one or more values using a random (with normal distribution) multiple
    /// </summary>
    public class FloatMutatorEvolver
    {
        public readonly Rng Rng = new LCPRNG_MMIX();
        public virtual ProbabilityGenerator ProbabilityGenerator { get; set; }


        public readonly double MinimumProbability = 1e-20;
        public double MultiMutateChance = 0.35;

        public double MeanIncrementRate = 3;
        public double MinStddevIncrementRate = 1.0;
        public double MaxStddevIncrementRate = 20.0;
        public virtual double StddevIncrementRate
        { // be more exploratory when the deviation is high
            get
            {
                var retval = MinStddevIncrementRate;
                var best = ProbabilityGenerator?.Best;
                if (best != null)
                {
                    var err = Math.Sqrt( best.Deviation );
                    retval = err.Clamp( MinStddevIncrementRate, MaxStddevIncrementRate );
                }
                return retval;
            }
        }

        public FloatMutatorEvolver() => Rng.UseFastScale = true;

        public void Evolve( Func<Chromosome> sampler, Chromosome output )
        {
            var parent = sampler();

            Array.Copy( parent.Probabilities, output.Probabilities, parent.ProbabilityCount );

            do
            {
                double rn = Rng.NextGaussian();
                rn *= StddevIncrementRate;
                var factor = Math.Abs( rn );

                int idx = Rng.Next( parent.ProbabilityCount );

                var newVal = parent.Probabilities[idx] * factor;

                newVal = newVal.ButNotLessThan( MinimumProbability );

                output.Probabilities[idx] = newVal;

            } while (Rng.NextDouble() < MultiMutateChance);

            //output.Probabilities.ChangeToProbabilities();
        }





    }
}
