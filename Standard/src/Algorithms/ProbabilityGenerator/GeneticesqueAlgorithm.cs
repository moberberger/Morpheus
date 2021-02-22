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
    public class GeneticesqueAlgorithm
    {
        public virtual Random Rng { get; set; } = DI.Default.Get<LCPRNG>();

        public virtual double MinimumProbability { get; set; } = 1e-20;

        public virtual double MutationChance { get; set; } = 0.05;
        public virtual double MultiMutateChance { get; set; } = 0.30;

        public virtual double MinMutationStdev { get; set; } = 1.0;
        public virtual double MaxMutationStdev { get; set; } = 40.0;
        public virtual double MutationStdev
        { // be more exploratory when the deviation is high
            get
            {
                return MinMutationStdev;
                //if (Best == null) return MaxMutationStdev;
                //var err = Math.Sqrt( Best.Deviation );
                //var retval = err.Clamp( MinMutationStdev, MaxMutationStdev );
                //return retval;
            }
        }





        private void Evolve( Func<Chromosome> generator, Chromosome result )
        {
            if (Rng.NextDouble() < MutationChance)
            {
                var basis = generator();
                Array.Copy( basis.Probabilities, result.Probabilities, basis.ProbabilityCount );

                do
                {
                    int idx = Rng.Next( basis.ProbabilityCount );

                    var factor = Math.Abs( Rng.NextGaussian( 1, MutationStdev ) );

                    var newVal = basis.Probabilities[idx] * factor;

                    newVal = Math.Max( newVal, MinimumProbability );

                    result.Probabilities[idx] = newVal;

                } while (Rng.NextDouble() < MultiMutateChance);

            }
            else
            {
                int len = result.ProbabilityCount;

                var p1 = generator();
                var p2 = generator();
                var idx1 = Rng.Next( len );
                var idx2 = Rng.Next( len );
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
