using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Morpheus
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ProbabilityGenerator
    {
        public State Config { get; set; }
        private List<int> lowerValueIndicies = new List<int>();
        private List<int> higherValueIndicies = new List<int>();

        /// <summary>
        /// Construct using an expected value and the established set of values.
        /// </summary>
        /// <param name="targetValue">
        /// Must be greater than the smallest of <see cref="values"/> and less than the largest
        /// of <see cref="values"/>
        /// </param>
        /// <param name="values">The values to associate probabilities with</param>
        public ProbabilityGenerator( double targetValue, params double[] values )
        {
            var cfg = DI.Default.Get<State>() ?? new State();
            cfg.TargetValue = targetValue;
            cfg.Values = values; // don't allocate more memory- these should never change

            Setup( cfg );
        }

        public ProbabilityGenerator( State config ) => Setup( config );

        private void Setup( State config )
        {
            Config = config;

            for (int i = 0; i < Config.ValueCount; i++)
            {
                if (Config.Values[i] <= Config.TargetValue)
                    lowerValueIndicies.Add( i );
                else if (Config.Values[i] > Config.TargetValue)
                    higherValueIndicies.Add( i );
            }
            Config.Pool = new ObjectPool<Chromosome>( Config.PopulationSize * Config.MutationCount * 2, () => new Chromosome( Config ) );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double[] Calculate()
        {
            Validate();

            // special and trivial case which always produces most accurate probabilities. No
            // heuristics possible, nor stochastics necessary- there's always exactly one
            // solution.
            if (Config.ValueCount == 2)
            {
                var vals = Config.Values;

                var p0 = (Config.TargetValue - vals[1]) / (vals[0] - vals[1]);
                var p1 = 1.0 - p0;

                Config.Best = new Chromosome( Config, p0, p1 );
            }
            else
            {
                ApplyHeuristicsAndStochastics();
                FixUsingHeuristic();
            }

            ErrorCheck();
            return Config.Best.ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        private void ApplyHeuristicsAndStochastics()
        {
            var db = Lib.Repeat( Config.PopulationSize, () => Config.Pool.Get() ).OrderBy( _x => _x.Error ).ToList();
            var nextDb = new List<Chromosome>();

            double error = double.MaxValue;
            foreach (var _ in Config.LoopUntilErrorSatisfactory())
            {
                nextDb.Add( db[0] ); // Elitism

                for (int i = 0; i < Config.PopulationSize; i++)
                {
                    var obj = db.Sample( _x => (double)_x.Error, true );
                    for (int j = 0; j < Config.MutationCount; j++)
                    {
                        var newObj = obj.Mutate();
                        nextDb.Add( newObj );
                    }
                }

                // ///////////////////////////////////////////////////////////////////////////
                // This algorithm is dependent on the cartesian product produced above
                nextDb.Sort();
                Config.Pool.Return( db.Mid( 1 ) ); // db[0] added as Elitism
                db.Clear();
                db.AddRange( nextDb.Take( Config.PopulationSize ) );
                Config.Pool.Return( nextDb.Mid( Config.PopulationSize ) );
                nextDb.Clear();
                // ///////////////////////////////////////////////////////////////////////////

                var smallest = db[0]; // list was sorted
                error = smallest.Error;
                Config.Best = smallest;

                Console.WriteLine( $"[{Config.IterationCount}] {smallest}" );
            }
            /// / Stochastics with Heuristic function for evaluation
            // var chromoTemplate = new Chromosome( Dimensionality, 64, ValuePV2Error ); var ga
            // = new GeneticAlgorithm( chromoTemplate ); ga.PoolSize = PoolSize; ga.ElitismCount
            // = ElitismCount; ga.MutationRate = MutationRate; ga.MutationStrength =
            // MutationStrength; var bestChromo = ga.Run( Generations );

            // decimal sum = 0; for (int i = 0; i Dimensionality; i++) sum += bestChromo[i]; for
            // (int i = 0; i Dimensionality; i++) probabilities[i] = (decimal)bestChromo[i] /
            // sum;
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
            var retval = Config.Best.Error;
            Config.Best.SetProbabilities( lowIdx, previousVals.Item1, highIdx, previousVals.Item2 );
            return retval;
        }

        private (double, double) FixTwoProbabilities( int lowIdx, int highIdx )
        {
            // Adjust these values based on the dot-product
            var v1 = Config.Values[lowIdx];
            var v2 = Config.Values[highIdx];
            var p1 = Config.Best[lowIdx];
            var p2 = Config.Best[highIdx];
            var c = p1 + p2;
            var d = p1 * v1 + p2 * v2 + Config.TargetValue - Config.Best.CalculatedValue;
            var newP1 = (d - c * v2) / (v1 - v2);
            var newP2 = c - newP1;

            // TODO: this is a translation of old proto code that needs refactoring
            if (newP1 > 0 && newP1 < 1 && newP2 > 0 && newP2 < 1)
                Config.Best.SetProbabilities( lowIdx, newP1, highIdx, newP2 );

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
            foreach (var p in Config.Best)
                if (p <= 0 || p >= 1)
                    throw new InvalidProgramException( $"Generated values which are not valid probabilities: {Config.Best.JoinAsString( ", " )}" );
        }

        /// <summary>
        /// 
        /// </summary>
        private void ErrorCheckDotProduct()
        {
            var sumProb = 0.0;
            var sumValue = 0.0;

            for (int i = 0; i < Config.ValueCount; i++)
            {
                sumProb += Config.Best[i];
                sumValue += Config.Best[i] * Config.Values[i];
            }

            if (!sumProb.IsClose( 1.0 ))
                throw new InvalidProgramException( $"Calculated the sum of probabilities to be {sumProb}. It should be 1.0" );

            if (!sumValue.IsClose( Config.Best.CalculatedValue ))
                throw new InvalidProgramException( $"Calculated an audit Value of {sumProb} that doesn't match the CalculatedValue of {Config.Best.CalculatedValue} found in the chromosome" );

            if (!Config.Best.CalculatedValue.IsClose( Config.TargetValue ))
                throw new InvalidProgramException( $"Calculated a value of {Config.Best.CalculatedValue} which is not equal to {Config.TargetValue}" );
        }
    }
}
