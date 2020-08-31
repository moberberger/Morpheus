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
        public int Generations = 100;

        /// <summary>
        /// 
        /// </summary>
        public int PopulationSize = 50;

        /// <summary>
        /// 
        /// </summary>
        public double MutationRate = 0.5;

        /// <summary>
        /// 
        /// </summary>
        public Random Rng { get; set; } = DI.Default.Get<Random>();

        /// <summary>
        /// The expected value of the dot-product
        /// </summary>
        public decimal ExpectedValue { get; set; }

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
            List<decimal[]> list1 = new List<decimal[]>();
            List<decimal[]> list2 = new List<decimal[]>();

            GenerateInitialProbs( list1 );
            GenerateInitialProbs( list2 );

            for (int q = 0; q < Generations; q++)
            {
                GenerateNextProbabilities( list1, list2 );
                GenerateNextProbabilities( list2, list1 );
            }
        }

        private void GenerateNextProbabilities( List<decimal[]> current, List<decimal[]> next )
        {
            for (int i = 0; i < PopulationSize; i++)
            {
                var probs = current.Sample( SimpleValueError, false );
                var nextP = next[i];

                for (int j = 0; j < Dimensionality; j++)
                {
                    nextP[j] = probs[j];
                    if (Rng.NextDouble() < MutationRate)
                    {
                        var rngVal = Rng.NextGaussian( 1, 0.5 );
                        var newVal = probs[j] + (decimal)rngVal;
                        if (newVal > 0)
                            nextP[j] = newVal;
                    }
                }
            }
        }

        private void GenerateInitialProbs( List<decimal[]> current )
        {
            for (int i = 0; i < PopulationSize; i++)
            {
                var parr = new decimal[Dimensionality];
                current.Add( parr );

                for (int j = 0; j < Dimensionality; j++)
                {
                    var rngVal = Math.Abs( Rng.NextGaussian( 2, 1 ) );
                    parr[j] = (decimal)rngVal;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void FixUsingHeuristic()
        {
            throw new NotImplementedException();
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
        public double SimpleValueError( decimal[] probabilities )
        {
            // TODO: figure out what to do about dividing probabilities by the sum of all probabilities
            1var val = probabilities.DotProduct( values );
            var delta = val - ExpectedValue;
            var err = delta * delta / ExpectedValue * 100;
            return (double)err;
        }
    }
}
