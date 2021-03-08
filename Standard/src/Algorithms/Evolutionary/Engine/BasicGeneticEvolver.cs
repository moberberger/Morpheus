using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Morpheus.Evolution
{
    /// <summary>
    /// 
    /// </summary>
    public class BasicGeneticEvolver
    {
        private LCPRNG_MMIX rng = new LCPRNG_MMIX();

        public double MutationChance = 0.05;

        public Func<ulong[], float> DeviationFunction;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviationFunction"></param>
        public BasicGeneticEvolver( Func<ulong[], float> deviationFunction = null )
        {
            DeviationFunction = deviationFunction;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="sampler"></param>
        /// <returns></returns>
        public float Evolve( ulong[] output, Func<double, ulong[]> sampler )
        {
            if (sampler != null)
            {
                var first = sampler( rng.NextDouble() );

                var chance = rng.NextDouble();
                if (chance < MutationChance)
                {
                    Mutate( first, output );
                }
                else
                {
                    var second = sampler( rng.NextDouble() );
                    Crossover( first, second, output );
                }
            }

            var deviation = DeviationFunction( output );
            return deviation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent1"></param>
        /// <param name="parent2"></param>
        /// <param name="output"></param>
        public void Crossover( ulong[] parent1, ulong[] parent2, ulong[] output )
        {
            int bitCount = output.Length << 6; // *64

            int rng1 = (int)(rng.State & int.MaxValue);
            int rng2 = (int)((rng.State >> 32) & int.MaxValue);

            int firstBit = rng1 % bitCount;
            int secondBit = rng2 % bitCount;

            Lib.Splice( parent1, parent2, firstBit, secondBit, output );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="output"></param>
        public void Mutate( ulong[] parent, ulong[] output )
        {
            ReadOnlySpan<ulong> buf = parent;
            buf.CopyTo( output );

            int bitCount = output.Length << 6; // *64
            int bitIndex = (int)rng.State % bitCount;
            int ulongIndex = bitIndex >> 6;
            var mask = 1UL << (bitIndex & 63);
            output[ulongIndex] ^= mask;
        }

    }
}
