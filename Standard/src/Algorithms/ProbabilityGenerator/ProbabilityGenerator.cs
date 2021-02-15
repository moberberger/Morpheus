using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace Morpheus
{
    public partial class ProbabilityGenerator
    {
        /// <summary>
        /// This set of input values represents what every generator must accept, at a minimum.
        /// Subclasses may add further information to the algorithm input.
        /// 
        /// This data is transcendent across all deviation functions.
        /// </summary>
        public class Input
        {
            /// <summary>
            /// Allows C/Asm to effect polymorphism
            /// </summary>
            protected readonly int Version = 0;

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
            public Input( double targetValue, params double[] values )
            {
                this.TargetValue = targetValue;
                this.Values = values ?? throw new ArgumentNullException( "Must pass in an array of values" );
                this.ValueCount = values.Length;
                if (ValueCount < 1) throw new InvalidEnumArgumentException( "You must pass in one or more values" );

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

        /// <summary>
        /// 
        /// </summary>
        public class Output
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
            public double[] Probabilities;

            /// <summary>
            /// The value associated with the best deviation found by the algorithm.
            /// </summary>
            public double CalculatedValue = double.MinValue;

            /// <summary>
            /// The best deviation found by the algorithm prior to fix-up. Just because there's
            /// a non-zero Deviation does not mean that the probabilities are valid.
            /// 
            /// To be valid, the <see cref="CalculatedValue"/> must equal the
            /// <see cref="Input.TargetValue"/> .
            /// </summary>
            public double Deviation = double.NaN;

            /// <summary>
            /// The DeviationFunction used to generate the Deviation. May be used later to
            /// re-create the details of the deviation based on the current probabilities.
            /// </summary>
            public DeviationFunction DeviationFn;

            /// <summary>
            /// Construct with the size of the probability array = input value array size
            /// </summary>
            /// <param name="input"></param>
            public Output( Input input )
            {
                Probabilities = (double[])Lib.CreatePopulatedArray( input.ValueCount, () => DI.Default.Get<Random>().NextDouble() );
            }


            /// <summary>
            /// How many probabilities are there?
            /// </summary>
            public int ProbabilityCount => Probabilities.Length;

            public override string ToString() => $"Err: ";
        }

        /// <summary>
        /// Identifies a Deviation Function, along with its parameters
        /// </summary>
        public abstract class DeviationFunction
        {
            protected DeviationFunction( int deviationFunctionId ) => DeviationFunctionId = deviationFunctionId;
            protected readonly int DeviationFunctionId;

            /// <summary>
            /// The total "deviation" for the input and output. See subclass for details.
            /// </summary>
            public double Out_Deviation;

            /// <summary>
            /// Set the values in the subclass
            /// </summary>
            /// <param name="_in"></param>
            /// <param name="_out"></param>
            public abstract void CalculateDeviation( Input _in, Output _out );
        }

        /// <summary>
        /// Identifies an Evolutionary Algorithm along with its parameters
        /// </summary>
        public abstract class EvolutionAlgorithm
        {
            public int IterationCount = 0;
            public double DeviationTolerance = .40;
            public bool TerminateCalculation { get; set; } = false;
            public Output Best { get; internal set; }
            protected IEnumerable<int> Iterate()
            {
                for (IterationCount = 0; (Best == null || Best.Deviation > DeviationTolerance) && !TerminateCalculation; IterationCount++)
                    yield return IterationCount;
            }

            public abstract Output Generate( Input _in, DeviationFunction _deviation );
        }

        /// <summary>
        /// Generated after the Deviation Function evaluates an input/output set
        /// </summary>
        public class DeviationDetail
        {
            /// <summary>
            /// The overall deviation
            /// </summary>
            public double Deviation;
        }




        /// <summary>
        /// A generalized deviation function which is meant to create as "smooth" of a curve as
        /// possible
        /// </summary>
        public class GeneralizedDeviationFunction : DeviationFunction
        {
            public GeneralizedDeviationFunction() : base( 0 ) { } // should also be treated as the "default"

            public double In_TargetValueAcceptableDeviationPercent = 0.05;

            public double In_ProbabilityDeviationWeight = 10;

            public double In_AngleDeviationWeight = 1000;

            public double In_DirectionCountPenalty = 10.0;
            public int In_DirectionCountTarget = 1;


            public double Out_ValueDeviation;
            public double Out_ProbabilityDeviation;
            public double Out_AngleDeviation;
            public double Out_DirectionChangeDeviation;

            public override string ToString() => throw new NotImplementedException();

            public override void CalculateDeviation( Input _in, Output _out )
            {
                double sumProb = 0;
                double sumProbSquared = 0;
                double sumValue = 0;
                double sumAngleSquared = 0;
                int dirChangeCount = 0;

                for (int i = 0; i < _in.ValueCount; i++)
                    sumProb += _out.Probabilities[i]; // used to normalize probabilities (0..1)

                for (int i = 0; i < _in.ValueCount; i++)
                {
                    _out.Probabilities[i] /= sumProb;
                    var p = _out.Probabilities[i];

                    var pp = (p - 1 / _in.ValueCount);
                    sumProbSquared += pp * pp;

                    sumValue += p * _in.Values[i];

                    if (i > 0)
                    {
                        var angle = p - _out.Probabilities[i - 1];
                        sumAngleSquared += angle * angle;

                        if (i < _in.ValueCount - 1 && Math.Sign( p - _out.Probabilities[i - 1] ) != Math.Sign( _out.Probabilities[i + 1] - p ))
                            dirChangeCount++;
                    }
                }

                _out.CalculatedValue = sumValue;

                // ERROR CALCULATION HERE Ratio- ideally this is zero, but is 1 when the target
                // is at the "Acceptable Deviation"
                Out_ValueDeviation = _out.CalculatedValue.DifferenceAsRatioOf( _in.TargetValue );
                Out_ValueDeviation /= In_TargetValueAcceptableDeviationPercent;
                Out_ValueDeviation *= Out_ValueDeviation;

                // I changed this before I could compile the xfer into Morpheus. Make sure this
                // works
                Out_ProbabilityDeviation = sumProbSquared * In_ProbabilityDeviationWeight / _in.ValueCount;
                Out_AngleDeviation = sumAngleSquared * In_AngleDeviationWeight / (_in.ValueCount - 1);

                Out_DirectionChangeDeviation = 0.0;
                if (In_DirectionCountTarget >= 0 && dirChangeCount != In_DirectionCountTarget)
                {
                    dirChangeCount = Math.Abs( In_DirectionCountTarget - dirChangeCount );
                    Out_DirectionChangeDeviation = Math.Pow( In_DirectionCountPenalty, dirChangeCount );
                }
                Out_Deviation = Math.Sqrt( Out_ValueDeviation + Out_ProbabilityDeviation + Out_AngleDeviation ) + Out_DirectionChangeDeviation;

                _out.Deviation = Out_Deviation;
            }
        }

        /// <summary>
        /// Evolve by mutating one or more values using a random (with normal distribution)
        /// multiple
        /// </summary>
        public class FloatingMutatorAlgorithm : EvolutionAlgorithm
        {
            public virtual double MinimumProbability { get; set; } = 1e-20;

            public virtual int PopulationSize { get; set; } = 300;

            public virtual int MutationCount { get; set; } = 2;

            public virtual double MultiMutateChance { get; set; } = 0.35;

            public virtual double MeanIncrementRate { get; set; } = 3;
            public virtual double MinStddevIncrementRate { get; set; } = 1.0;
            public virtual double MaxStddevIncrementRate { get; set; } = 20.0;
            public virtual double StddevIncrementRate
            { // be more exploratory when the deviation is high
                get
                {
                    if (Best == null) return MaxStddevIncrementRate;
                    var err = Math.Sqrt( Best.Deviation );
                    var retval = err.Clamp( MinStddevIncrementRate, MaxStddevIncrementRate );
                    return retval;
                }
            }


            /// <summary>
            /// Generate the Output based on the Input and the parameters provided
            /// </summary>
            /// <param name="input"></param>
            /// <param name="deviationFn"></param>
            /// <returns></returns>
            public override Output Generate( Input input, DeviationFunction deviationFn )
            {
                var pool = new ObjectPool<Output>( PopulationSize * MutationCount * 2, () => new Output( input ) );

                var sampleSet = Lib.Repeat( PopulationSize, () => pool.Get() )
                                   .OrderBy( _x => _x.Deviation )
                                   .ToList();

                var resultSet = new List<Output>();

                TerminateCalculation = false;
                double deviation = double.MaxValue;
                foreach (var _ in Iterate())
                {
                    // Generate new outputs
                    for (int i = 0; i < PopulationSize; i++)
                    {
                        var obj = sampleSet.Sample( _x => (double)_x.Deviation, true );
                        for (int j = 0; j < MutationCount; j++)
                        {
                            var result = pool.Get();
                            Mutate( obj, result );
                            deviationFn.CalculateDeviation( input, result );
                            resultSet.Add( result );
                        }
                    }

                    // Process outputs into new SampleSet
                    resultSet.Add( sampleSet[0] ); // Elitism
                    resultSet.Sort();

                    pool.Return( sampleSet.Mid( 1 ) ); // [0] added as Elitism
                    sampleSet.Clear();

                    sampleSet.AddRange( resultSet.Take( PopulationSize ) );
                    pool.Return( resultSet.Mid( PopulationSize ) );
                    resultSet.Clear();

                    var smallest = sampleSet[0]; // list was sorted
                    deviation = smallest.Deviation;
                    Best = smallest;

                    Console.WriteLine( $"[{IterationCount}] {smallest}" );
                }

                return Best;
            }

            private void Mutate( Output _in, Output _out )
            {
                do
                {
                    int idx = Rng.Default.Next( _in.ProbabilityCount );

                    var factor = Math.Abs( (double)Rng.Default.NextGaussian( MeanIncrementRate, StddevIncrementRate ) );

                    var newVal = _in.Probabilities[idx] * factor;

                    newVal = Math.Max( newVal, MinimumProbability );

                    _out.Probabilities[idx] = newVal;

                } while (Rng.Default.NextDouble() < MultiMutateChance);
            }
        }


        public Input _Input;
        public Output _Output;
        public DeviationFunction _DeviationFunction;
        public EvolutionAlgorithm _Algorithm;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="deviationFn"></param>
        /// <param name="targetValue"></param>
        /// <param name="values"></param>
        public ProbabilityGenerator( EvolutionAlgorithm algorithm, DeviationFunction deviationFn, double targetValue, params double[] values )
        {
            _Input = new Input( targetValue, values );
            _DeviationFunction = deviationFn;
            _Algorithm = algorithm;
        }

        public Output Generate()
        {
            _Output = _Algorithm.Generate( _Input, _DeviationFunction );
            return _Output;
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
