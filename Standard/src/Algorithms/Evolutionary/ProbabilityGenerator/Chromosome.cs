﻿using System;
using System.Collections.Generic;

using System.Linq;

namespace Morpheus.Evolution.PGNS
{
    /// <summary>
    /// 
    /// </summary>
    public class Chromosome : Evolution.Chromosome
    {
        /// <summary>
        /// The probabilities associated with the best deviation found by the algorithm
        /// 
        /// this and only this field must be set prior to calling the deviation function
        /// </summary>
        private readonly double[] RawProbabilities;

        /// <summary>
        /// How many probabilities are there?
        /// </summary>
        public int ProbabilityCount => RawProbabilities.Length;

        /// <summary>
        /// The value associated with the best deviation found by the algorithm.
        /// </summary>
        public double CalculatedValue { get; internal set; } = double.MinValue;


        /// <summary>
        /// Cannot be constructed externally- use creator functions
        /// </summary>
        private Chromosome( int size ) => RawProbabilities = new double[size];


        /// <summary>
        /// The creator for this type of chromosome
        /// </summary>
        /// <param name="input"></param>
        /// <param name="initialized"></param>
        /// <returns></returns>
        public static Chromosome Create( Config input, bool initialized )
        {
            var ch = new Chromosome( input.ValueCount );

            if (initialized)
            {
                for (int i = 0; i < input.ValueCount; i++)
                {
                    var x = DI.Default.Get<Random>().NextGaussian( 0, 1 );
                    x = Math.Abs( x );
                    ch.RawProbabilities[i] = x;
                }
                ch.RawProbabilities.ChangeToProbabilities();
            }

            return ch;
        }


        /// <summary>
        /// Copy values from another chromosome into this one, making them functionally
        /// identical
        /// </summary>
        /// <param name="other"></param>
        public override void CopyTo( Evolution.Chromosome other )
        {
            var chromo = other as Chromosome ?? throw new ArgumentException( $"other is wrong type: {other.GetType()}" );
            base.CopyTo( other );

            Array.Copy( RawProbabilities, chromo.RawProbabilities, ProbabilityCount );
            chromo.CalculatedValue = CalculatedValue;
        }

        public IEnumerable<
    }

}
