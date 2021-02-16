using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// 
    /// </summary>
    public class Output : IComparable<Output>, IComparable
    {
        /// <summary>
        /// Allows C/Asm to effect polymorphism
        /// </summary>
        protected readonly int Version = 0;

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
        /// The best deviation found by the algorithm prior to fix-up. Just because there's a
        /// non-zero Deviation does not mean that the probabilities are valid.
        /// 
        /// To be valid, the <see cref="CalculatedValue"/> must equal the
        /// <see cref="Input.TargetValue"/> .
        /// </summary>
        public double Deviation = double.NaN;

        /// <summary>
        /// Construct with the size of the probability array = input value array size
        /// </summary>
        /// <param name="input"></param>
        public Output( Input input )
        {
            Probabilities = (double[])Lib.CreatePopulatedArray( input.ValueCount, () => Math.Abs( DI.Default.Get<Random>().NextGaussian( 0, 1 ) ) );
            Probabilities.ChangeToProbabilities();
        }

        /// <summary>
        /// Construct with the size of the probability array = input value array size
        /// </summary>
        /// <param name="other"></param>
        public Output( Output other )
        {
            Probabilities = (double[])other.Probabilities.Clone();
        }


        /// <summary>
        /// How many probabilities are there?
        /// </summary>
        public int ProbabilityCount => Probabilities.Length;

        /// <summary>
        /// The deviation is what determines order
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo( Output other ) => Math.Sign( Deviation - other.Deviation );

        int IComparable.CompareTo( object obj ) => CompareTo( (obj as Output) ?? throw new ArgumentException( $"Invalid type: {obj.GetType()}" ) );

        public override string ToString() => $"dev: {Deviation:N6}";

    }

}
