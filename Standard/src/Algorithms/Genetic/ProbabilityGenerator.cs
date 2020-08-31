using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Morpheus
{
    /// <summary>
    /// 
    /// </summary>
    public class ProbabilityGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        public Random Rng { get; set; } = DI.Default.Get<Random>();

        private double expectedValue;
        private double[] values;
        private double[] probabilities;

        /// <summary>
        /// Construct using an expected value and the established set of values.
        /// </summary>
        /// <param name="expectedValue">
        /// Must be greater than the smallest of <see cref="values"/> and less than the largest
        /// of <see cref="values"/>
        /// </param>
        /// <param name="values">The values to associate probabilities with</param>
        public ProbabilityGenerator( double expectedValue, params double[] values )
        {
            this.expectedValue = expectedValue;
            this.values = values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double[] Calculate()
        {
            Validate();

            probabilities = new double[values.Length];

            if (values.Length == 2) // special and trivial case
            {
                probabilities[0] = (expectedValue - values[1]) / (values[0] - values[1]);
                probabilities[1] = 1 - probabilities[0];
            }
            else
            {
                ApplyHeuristicsAndStochastics();
                FixUsingHeuristic();
            }

            ErrorCheck();
            return probabilities;
        }


        /// <summary>
        /// 
        /// </summary>
        private void Validate()
        {
            bool foundLesser = false, foundGreater = false;

            foreach (var v in values)
            {
                if (v < expectedValue) foundLesser = true;
                else if (v > expectedValue) foundGreater = true;
            }

            if (!foundLesser || !foundGreater)
                throw new InvalidOperationException( $"Invalid Expected Value: BOTH should be true---> Found Lesser: {foundLesser}, FoundGreater: {foundGreater}" );
        }

        /// <summary>
        /// 
        /// </summary>
        private void ErrorCheck()
        {
            double actualValue = values.DotProduct( probabilities );

            if (expectedValue != actualValue)
                throw new InvalidProgramException( $"The Expected Value {expectedValue} does not equal the calculated value {actualValue}." );
        }

        private void ApplyHeuristicsAndStochastics()
        {
            throw new NotImplementedException();
        }

        private void FixUsingHeuristic()
        {
            throw new NotImplementedException();
        }
    }
}
