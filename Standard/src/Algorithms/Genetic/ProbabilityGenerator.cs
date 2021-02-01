using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public Random Rng { get; set; } = DI.Default.Get<Random>();

        /// <summary>
        /// The expected value of the dot-product
        /// </summary>
        public decimal ExpectedValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal CalculatedValue { get => values.DotProduct( probabilities ); }

        /// <summary>
        /// How many values/probabilities are there
        /// </summary>
        public int Dimensionality { get => values.Length; }

        /// <summary>
        /// Get a copy of the probabilities generated
        /// </summary>
        public decimal[] Probabilities { get => (decimal[])probabilities.Clone(); }


        private decimal[] values;
        private decimal[] probabilities;
        private decimal[] tempBuffer;

        private List<int> lowerValueIndicies = new List<int>();
        private List<int> higherValueIndicies = new List<int>();

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
            this.values = (decimal[])values.Clone();

            for (int i = 0; i < Dimensionality; i++)
            {
                if (values[i] < ExpectedValue)
                    lowerValueIndicies.Add( i );
                else if (values[i] > ExpectedValue)
                    higherValueIndicies.Add( i );
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public decimal[] Calculate()
        {
            Validate();

            probabilities = new decimal[Dimensionality];
            tempBuffer = new decimal[Dimensionality];

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
            // Stochastics with Heuristic function for evaluation
            var chromoTemplate = new Chromosome( Dimensionality, 64, ValuePV2Error );
            var ga = new GeneticAlgorithm( chromoTemplate );
            ga.PoolSize = PoolSize;
            ga.ElitismCount = ElitismCount;
            ga.MutationRate = MutationRate;
            ga.MutationStrength = MutationStrength;
            var bestChromo = ga.Run( Generations );

            decimal sum = 0;
            for (int i = 0; i < Dimensionality; i++)
                sum += bestChromo[i];
            for (int i = 0; i < Dimensionality; i++)
                probabilities[i] = (decimal)bestChromo[i] / sum;
        }

        /// <summary>
        /// 
        /// </summary>
        private void FixUsingHeuristic()
        {
            var bestIdx = (-1, -1); // lower index / upper index
            var bestErr = double.MaxValue;

            // find low and high indicies (related to ExpectedValue)
            for (int j = 0; j < lowerValueIndicies.Count; j++)
            {
                for (int k = 0; k < higherValueIndicies.Count; k++)
                {
                    var lowerIdx = lowerValueIndicies[j];
                    var higherIdx = higherValueIndicies[k];
                    double err = GetErrorBySwitchingIndicies( lowerIdx, higherIdx );
                    if (err < bestErr)
                    {
                        bestErr = err;
                        bestIdx = (lowerIdx, higherIdx);
                    }
                }
            }

            FixTwoProbabilities( bestIdx.Item1, bestIdx.Item2 );
        }

        private double GetErrorBySwitchingIndicies( int lowIdx, int highIdx )
        {
            var previousVals = FixTwoProbabilities( lowIdx, highIdx );
            var retval = ValuePV2Error( probabilities );
            probabilities[lowIdx] = previousVals.Item1;
            probabilities[highIdx] = previousVals.Item2;
            return retval;
        }

        private (decimal, decimal) FixTwoProbabilities( int lowIdx, int highIdx )
        {
            // Adjust these values based on the dot-product
            var v1 = values[lowIdx];
            var v2 = values[highIdx];
            var p1 = probabilities[lowIdx];
            var p2 = probabilities[highIdx];
            var c = p1 + p2;
            var d = p1 * v1 + p2 * v2 + ExpectedValue - CalculatedValue;
            var newP1 = (d - c * v2) / (v1 - v2);
            var newP2 = c - newP1;

            if (newP1 > 0 && newP1 < 1 && newP2 > 0 && newP2 < 1)
            {
                probabilities[lowIdx] = newP1;
                probabilities[highIdx] = newP2;
            }
            return (p1, p2);
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
        private void ErrorCheck()
        {
            ErrorCheckDotProduct();
            ErrorCheckValidProbabilities();
        }





        /// <summary>
        /// 
        /// </summary>
        private void ValidateInRange()
        {
            if (lowerValueIndicies.Count == 0 || higherValueIndicies.Count == 0)
                throw new InvalidOperationException( $"There must be at least one Value that is LOWER than the expected value AND one value greater than the expected value" );
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
            decimal sumProbabilities = 0, sumProduct = 0;

            for (int i = 0; i < Dimensionality; i++)
            {
                sumProbabilities += probabilities[i];
                sumProduct += probabilities[i] * values[i];
            }
            var val = sumProduct / sumProbabilities;
            var delta = val - ExpectedValue;
            var err = delta * delta;
            return (double)err;
        }

        /// <summary>
        /// no obj allocation translation of a chromo into the tempBuffer
        /// </summary>
        /// <param name="chromo"></param>
        /// <returns></returns>
        public double ValuePV2Error( Chromosome chromo )
        {
            decimal sum = 0;
            for (int i = 0; i < Dimensionality; i++)
            {
                decimal counter = chromo[i];
                tempBuffer[i] = counter;
                sum += counter;
            }
            for (int i = 0; i < Dimensionality; i++)
                tempBuffer[i] /= sum;

            return ValuePV2Error( tempBuffer );
        }

        /// <summary>
        /// no memory allocation generation of the error in the current values with the
        /// specified probabilities
        /// </summary>
        /// <param name="probs"></param>
        /// <returns></returns>
        public double ValuePV2Error( decimal[] probs )
        {
            decimal sumProduct = 0;
            decimal maxV2 = decimal.MinValue, minV2 = decimal.MaxValue;

            for (int i = 0; i < Dimensionality; i++)
            {
                var p = probs[i];
                var v = values[i];

                sumProduct += p * v;

                maxV2 = Math.Max( maxV2, v * v );
                minV2 = Math.Min( minV2, v * v );
            }
            var delta = (sumProduct - ExpectedValue) * 10;
            var err1 = (double)(delta * delta);
            // return err1;

            decimal sumPV2 = 0;
            for (int i = 0; i < Dimensionality; i++)
            {
                var p = probs[i];
                var v = values[i];
                var e = maxV2 - p * v * v;
                sumPV2 += e * e;
            }
            var range = (maxV2 - minV2);
            var valPV2 = sumPV2 / range;
            var err2 = Math.Sqrt( (double)valPV2 );

            return err1 + err2;
        }

        /// <summary>
        /// 
        /// </summary>
        public int PoolSize = 250;
        /// <summary>
        /// 
        /// </summary>
        public int Generations = 50;
        /// <summary>
        /// 
        /// </summary>
        public int ElitismCount = 3;
        /// <summary>
        /// 
        /// </summary>
        public double MutationRate = 0.5;
        /// <summary>
        /// 
        /// </summary>
        public double MutationStrength = 0.5;
    }
}
