using System;

namespace Morpheus.Evolution
{
    /// <summary>
    /// This set of input values represents what every generator must accept, at a minimum.
    /// Subclasses may add further information to the algorithm input.
    /// 
    /// This data is transcendent across all deviation functions.
    /// </summary>
    public class ProbGenInput
    {
        /// <summary>
        /// The value that the algorithm should try to achieve
        /// </summary>
        public readonly double TargetValue = 222;

        /// <summary>
        /// The values that should be used. Implicitly determines <see cref="ValueCount"/>
        /// </summary>
        public readonly double[] Values;

        /// <summary>
        /// CONST- Set in constructor equal to the length of the Values array (used for
        /// possible interop)
        /// </summary>
        public readonly int ValueCount;

        /// <summary>
        /// Create the parameters used as Input into an evolutionary algorithm
        /// </summary>
        /// <param name="targetValue">
        /// What should the evolutionary algorithm target as the dot-product of values and
        /// probabilities
        /// </param>
        /// <param name="values">The values to determine probabilities for</param>
        public ProbGenInput( double targetValue, params double[] values )
        {
            if (!targetValue.IsBetween( double.Epsilon, double.PositiveInfinity )) throw new ArgumentException( $"TargetValue must be a positive number, not {targetValue}" );
            this.TargetValue = targetValue;
            this.Values = values ?? throw new ArgumentNullException( "Must pass in an array of values" );
            this.ValueCount = values.Length;
            if (ValueCount < 1) throw new ArgumentException( "You must pass in one or more values" );

            bool below = false, above = false;
            for (int i = 0; i < ValueCount && (!below || !above); i++)
            {
                below |= values[i] < targetValue;
                above |= values[i] > targetValue;
            }

            if (!below || !above) // didn't find both an above and a below
            {
                // its still possible that all values are equal to the target value
                for (int i = 0; i < ValueCount; i++)
                {
                    if (values[i] != targetValue) // nope... they aren't all equal
                        throw new InvalidOperationException( $"MUST have one Value lower than {TargetValue} and one greater. ALTERNATELY, all Values may EQUAL {TargetValue}." );
                }
            }
        }
    }
}
