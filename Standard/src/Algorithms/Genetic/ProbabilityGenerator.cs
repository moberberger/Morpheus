using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        public int Generations = 10000;

        /// <summary>
        /// 
        /// </summary>
        public int PopulationSize = 200;

        /// <summary>
        /// 
        /// </summary>
        public double MutationRate = 0.1;

        /// <summary>
        /// 
        /// </summary>
        public Random Rng { get; set; } = DI.Default.Get<Random>();

        /// <summary>
        /// The expected value of the dot-product
        /// </summary>
        public decimal ExpectedValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal CalculatedValue { get => values.DotProduct( probabilities ); }





        private decimal[] values;
        private decimal[] probabilities;

        /// <summary>
        /// How many values/probabilities are there
        /// </summary>
        public int Dimensionality { get => values.Length; }

        /// <summary>
        /// Construct using an expected value and the established set of values.
        /// </summary>
        /// <param name="expectedValue">
        /// Must be greater than the smallest of <see cref="values"/> and less than the largest
        /// of <see cref="values"/>
        /// </param>
        /// <param name="values">The values to associate probabilities with</param>
        public ProbabilityGenerator( decimal expectedValue, params decimal[] values )
        {
            this.ExpectedValue = expectedValue;
            this.values = values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public decimal[] Calculate()
        {
            Validate();

            probabilities = new decimal[values.Length];

            // special and trivial case which always produces most accurate probabilities. No
            // heuristics possible, nor stochastics necessary- there's always exactly one
            // solution.
            if (values.Length == 2)
            {
                probabilities[0] = (ExpectedValue - values[1]) / (values[0] - values[1]);
                probabilities[1] = 1.0m - probabilities[0];
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
        private void ApplyHeuristicsAndStochastics()
        {
            var current = new int[Dimensionality];
            for (int i = 0; i < current.Length; i++)
                current[i] = 1;

            double bestError;
            int bestIndex;

            for (int i = 0; i < Generations; i++)
            {
                bestError = double.MaxValue;
                bestIndex = -1;

                for (int j = 0; j < Dimensionality; j++)
                {
                    current[j]++;
                    var error = SimpleValueError( current );
                    current[j]--;

                    if (error < bestError)
                    {
                        bestError = error;
                        bestIndex = j;
                    }
                }
                if (bestIndex == -1)
                    break;

                current[bestIndex]++;
            }

            decimal sum = current.Sum();
            for (int i = 0; i < Dimensionality; i++)
                probabilities[i] = (decimal)current[i] / sum;
        }

        /// <summary>
        /// 
        /// </summary>
        private void FixUsingHeuristic()
        {
            int lowIdx = 0, highIdx = 0;
            for (int i = 0; i < Dimensionality; i++)
            {
                if (values[i] < ExpectedValue) lowIdx = i;
                if (values[i] > ExpectedValue) highIdx = i;
            }

            var v1 = values[lowIdx];
            var v2 = values[highIdx];
            var p1 = probabilities[lowIdx];
            var p2 = probabilities[highIdx];

            var c = p1 + p2;
            var d = p1 * v1 + p2 * v2 + ExpectedValue - CalculatedValue;
            var p11 = (d - c * v2) / (v1 - v2);
            var p22 = c - p11;

            probabilities[lowIdx] = p11;
            probabilities[highIdx] = p22;
        }


        /// <summary>
        /// 
        /// </summary>
        private void Validate()
        {
            ValidateInRange();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ValidateInRange()
        {
            bool foundLesser = false, foundGreater = false;

            foreach (var v in values)
            {
                if (v < ExpectedValue)
                    if (foundGreater)
                        return;
                    else
                        foundLesser = true;
                else if (v > ExpectedValue)
                    if (foundLesser)
                        return;
                    else
                        foundGreater = true;
            }

            throw new InvalidOperationException( $"Invalid Expected Value: BOTH should be true---> Found Lesser: {foundLesser}, FoundGreater: {foundGreater}" );
        }

        /// <summary>
        /// 
        /// </summary>
        private void ErrorCheck()
        {
            ErrorCheckDotProduct();
            ErrorCheckValidProbabilities();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ErrorCheckValidProbabilities()
        {
            foreach (var p in probabilities)
                if (p <= 0 || p >= 1)
                    throw new InvalidProgramException( $"Generated values which are not valid probabilities: {probabilities.JoinAsString( ", " )}" );
        }

        /// <summary>
        /// 
        /// </summary>
        private void ErrorCheckDotProduct()
        {
            decimal actualValue = values.DotProduct( probabilities );
            var delta = Math.Abs( actualValue - ExpectedValue );

            if (delta > 0.000000000001m)
                throw new InvalidProgramException( $"The Expected Value {ExpectedValue} does not equal the calculated value {actualValue}." );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="probabilities"></param>
        /// <returns></returns>
        public double SimpleValueError( int[] probabilities )
        {
            decimal sum = 0, sumProduct = 0;

            for (int i = 0; i < Dimensionality; i++)
            {
                sum += probabilities[i];
                sumProduct += probabilities[i] * values[i];
            }
            var val = sumProduct / sum;
            var delta = val - ExpectedValue;
            var err = delta * delta;
            return (double)err;
        }
    }
}
