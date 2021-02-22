using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Morpheus.Evolution.PGNS
{
    /// <summary>
    /// Evolve by mutating one or more values using a random (with normal distribution)
    /// multiple
    /// </summary>
    public class FloatMutatorEvolver
    {
        public virtual Random Rng { get; set; } = DI.Default.Get<LCPRNG>();

        public virtual double MinimumProbability { get; set; } = 1e-20;
        public virtual double MultiMutateChance { get; set; } = 0.35;

        public virtual double MeanIncrementRate { get; set; } = 3;
        public virtual double MinStddevIncrementRate { get; set; } = 1.0;
        public virtual double MaxStddevIncrementRate { get; set; } = 20.0;
        public virtual double StddevIncrementRate
        { // be more exploratory when the deviation is high
            get
            {
                return MinStddevIncrementRate;
                //if (Best == null) return MaxStddevIncrementRate;
                //var err = Math.Sqrt( Best.Deviation );
                //var retval = err.Clamp( MinStddevIncrementRate, MaxStddevIncrementRate );
                //return retval;
            }
        }


        public void Evolve( Func<Chromosome> sampler, Chromosome output )
        {
            var parent = sampler();

            Array.Copy( parent.Probabilities, output.Probabilities, parent.ProbabilityCount );

            do
            {
                int idx = Rng.Next( parent.ProbabilityCount );

                var factor = Math.Abs( Rng.NextGaussian( MeanIncrementRate, StddevIncrementRate ) );

                var newVal = parent.Probabilities[idx] * factor;

                newVal = newVal.ButNotLessThan( MinimumProbability );

                output.Probabilities[idx] = newVal;

            } while (Rng.NextDouble() < MultiMutateChance);

            output.Probabilities.ChangeToProbabilities();
        }





    }
}
