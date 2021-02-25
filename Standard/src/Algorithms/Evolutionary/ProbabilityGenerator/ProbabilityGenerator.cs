using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace Morpheus
{
    public class ProbabilityGenerator : Evolution.Engine<Evolution.PGNS.Chromosome, Evolution.PGNS.Config, Evolution.PGNS.DeviationDetail>
    {
        public Evolution.PGNS.Config Config;
        public Evolution.PGNS.DeviationFunction Deviation = new Evolution.PGNS.DeviationFunction();
        public Evolution.PGNS.FloatMutatorEvolver PGEvolver = new Evolution.PGNS.FloatMutatorEvolver();

        public ProbabilityGenerator( double targetValue, params double[] values )
        {
            Config = new Evolution.PGNS.Config( targetValue, values );

            InputConfig = Config;
            DeviationFunction = Deviation.CalculateDeviation;
            Evolver = PGEvolver.Evolve;
            ChromosomeCreator = Evolution.PGNS.Chromosome.Create;

            PGEvolver.ProbabilityGenerator = this;
            Resize( 256 );
        }
    }
}



#if false

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
        public PGState State { get; private set; }

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
            var cfg = DI.Default.Get<PGState>() ?? new PGState();
            cfg.TargetValue = targetValue;
            cfg.Values = values; // don't allocate more memory- these should never change

            Setup( cfg );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public ProbabilityGenerator( PGState state ) => Setup( state );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void Setup( PGState state )
        {
            State = state ?? throw new ArgumentNullException( "state" );

            for (int i = 0; i < State.ValueCount; i++)
            {
                if (State.Values[i] <= State.TargetValue)
                    lowerValueIndicies.Add( i );
                else if (State.Values[i] > State.TargetValue)
                    higherValueIndicies.Add( i );
            }

            State.Pool = new ObjectPool<Chromosome>( State.PopulationSize * State.MutationCount * 2, () => new Chromosome( State ) );
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
            if (State.ValueCount == 2)
            {
                var vals = State.Values;

                var p0 = (State.TargetValue - vals[1]) / (vals[0] - vals[1]);
                var p1 = 1.0 - p0;

                State.Best = new Chromosome( State, p0, p1 );
            }
            else
            {
                ApplyHeuristicsAndStochastics();
                FixUsingHeuristic();
            }

            ErrorCheck();
            return State.Best.ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        private void ApplyHeuristicsAndStochastics()
        {
            var sampleSet = Lib.Repeat( State.PopulationSize, () => State.Pool.Get() ).OrderBy( _x => _x.Error ).ToList();
            var resultSet = new List<Chromosome>();

            State.TerminateCalculation = false;
            double error = double.MaxValue;
            foreach (var _ in State.LoopUntilErrorSatisfactory())
            {
                for (int i = 0; i < State.PopulationSize; i++)
                {
                    var obj = sampleSet.Sample( _x => (double)_x.Error, true );
                    for (int j = 0; j < State.MutationCount; j++)
                    {
                        var newObj = obj.Mutate();
                        resultSet.Add( newObj );
                    }
                }

                // ///////////////////////////////////////////////////////////////////////////
                // very coupled algorithm here- fix
                resultSet.Add( sampleSet[0] ); // Elitism
                resultSet.Sort();

                State.Pool.Return( sampleSet.Mid( 1 ) ); // [0] added as Elitism
                sampleSet.Clear();

                sampleSet.AddRange( resultSet.Take( State.PopulationSize ) );
                State.Pool.Return( resultSet.Mid( State.PopulationSize ) );
                resultSet.Clear();
                // ///////////////////////////////////////////////////////////////////////////

                var smallest = sampleSet[0]; // list was sorted
                error = smallest.Error;
                State.Best = smallest;

                Console.WriteLine( $"[{State.IterationCount}] {smallest}" );
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
            var retval = State.Best.Error;
            State.Best.SetProbabilities( lowIdx, previousVals.Item1, highIdx, previousVals.Item2 );
            return retval;
        }

        private (double, double) FixTwoProbabilities( int lowIdx, int highIdx )
        {
            // Adjust these values based on the dot-product
            var v1 = State.Values[lowIdx];
            var v2 = State.Values[highIdx];
            var p1 = State.Best[lowIdx];
            var p2 = State.Best[highIdx];
            var c = p1 + p2;
            var d = p1 * v1 + p2 * v2 + State.TargetValue - State.Best.CalculatedValue;
            var newP1 = (d - c * v2) / (v1 - v2);
            var newP2 = c - newP1;

            // TODO: this is a translation of old proto code that needs refactoring
            if (newP1 > 0 && newP1 < 1 && newP2 > 0 && newP2 < 1)
                State.Best.SetProbabilities( lowIdx, newP1, highIdx, newP2 );

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
            foreach (var p in State.Best)
                if (p <= 0 || p >= 1)
                    throw new InvalidProgramException( $"Generated values which are not valid probabilities: {State.Best.JoinAsString( ", " )}" );
        }

        /// <summary>
        /// 
        /// </summary>
        private void ErrorCheckDotProduct()
        {
            var sumProb = 0.0;
            var sumValue = 0.0;

            for (int i = 0; i < State.ValueCount; i++)
            {
                sumProb += State.Best[i];
                sumValue += State.Best[i] * State.Values[i];
            }

            if (!sumProb.IsClose( 1.0 ))
                throw new InvalidProgramException( $"Calculated the sum of probabilities to be {sumProb}. It should be 1.0" );

            if (!sumValue.IsClose( State.Best.CalculatedValue ))
                throw new InvalidProgramException( $"Calculated an audit Value of {sumProb} that doesn't match the CalculatedValue of {State.Best.CalculatedValue} found in the chromosome" );

            if (!State.Best.CalculatedValue.IsClose( State.TargetValue ))
                throw new InvalidProgramException( $"Calculated a value of {State.Best.CalculatedValue} which is not equal to {State.TargetValue}" );
        }
    }
}

#endif
