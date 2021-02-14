using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// This data is transcendent across all error functions.
        /// </summary>
        public class CoreInputValues
        {
            /// <summary>
            /// Allows C/Asm to effect polymorphism
            /// </summary>
            protected readonly int InputTypeId = 0;

            /// <summary>
            /// The value that the algorithm should try to achieve
            /// </summary>
            public readonly double TargetValue = 222;

            /// <summary>
            /// The values that should be used. Implicitly determines <see cref="ValueCount"/>
            /// </summary>
            public readonly double[] Values = new double[] { 1, 5, 10, 25, 50, 100, 250, 500, 1000, 5000, 1000000 };

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
            public CoreInputValues( double targetValue, double[] values )
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
        public class CoreOutputValues
        {
            /// <summary>
            /// Allows C/Asm to effect polymorphism
            /// </summary>
            protected readonly int OutputTypeId = 0;

            /// <summary>
            /// A copy of the TargetValue used as input. Used to determine validity of the
            /// CalculatedValue
            /// </summary>
            public double TargetValue = double.NaN;

            /// <summary>
            /// The value associated with the best error found by the algorithm. See
            /// <see cref="IsValid"/> for interpretation.
            /// </summary>
            public double CalculatedValue = double.MinValue;

            /// <summary>
            /// The probabilities associated with the best error found by the algorithm
            /// </summary>
            public double[] Probabilities;

            /// <summary>
            /// The best error found by the algorithm prior to fix-up. Just because there's a
            /// non-zero Error does not mean that <see cref="IsValid"/> is false.
            /// 
            /// This facilitates using two algorithms- one stochastic with heuristics, and one
            /// strictly mathematical and precise
            /// </summary>
            public double Error = double.NaN;

            /// <summary>
            /// Can only be valid if these two values are equal
            /// </summary>
            public bool IsValid => CalculatedValue == TargetValue && Probabilities != null;
        }


        /// <summary>
        /// This Id allows assembly/C to effect polymorphism of error function in optimized
        /// implementations
        /// </summary>
        public abstract class CoreErrorParameters
        {
            protected CoreErrorParameters( int errorFunctionId ) => ErrorFunctionId = errorFunctionId;
            protected readonly int ErrorFunctionId;

            /// <summary>
            /// The total "error" for the input and output. See subclass for details.
            /// </summary>
            public double Out_Error;

            /// <summary>
            /// Set the values in the subclass 
            /// </summary>
            /// <param name="_in"></param>
            /// <param name="_out"></param>
            public abstract void CalculateError( CoreInputValues _in, CoreOutputValues _out );
        }

        /// <summary>
        /// A generalized error function which is meant to create as "smooth" of a curve as
        /// possible
        /// </summary>
        public class GeneralizedErrorFunction : CoreErrorParameters
        {
            public const double MinimumProbability = 1e-20;

            public GeneralizedErrorFunction() : base( 0 ) { } // should also be treated as the "default"

            public double In_TargetValueAcceptableErrorPercent = 0.05;

            public double In_ProbabilityErrorWeight = 10;

            public double In_AngleErrorWeight = 1000;

            public double In_DirectionCountPenalty = 10.0;
            public int In_DirectionCountTarget = 1;


            public double Out_ValueError;
            public double Out_ProbabilityError;
            public double Out_AngleError;
            public double Out_DirectionChangeError;

            public override string ToString() => throw new NotImplementedException();

            public override void CalculateError( CoreInputValues _in, CoreOutputValues _out )
            {
                _out.TargetValue = _in.TargetValue;

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

                // ERROR CALCULATION HERE Ratio- ideally this is zero, but is 1 when the target is
                // at the "Acceptable Error"
                Out_ValueError = _out.CalculatedValue.DifferenceAsRatioOf( _in.TargetValue );
                Out_ValueError /= In_TargetValueAcceptableErrorPercent;
                Out_ValueError *= Out_ValueError;

                // I changed this before I could compile the xfer into Morpheus. Make sure this works
                Out_ProbabilityError = sumProbSquared * In_ProbabilityErrorWeight / _in.ValueCount;
                Out_AngleError = sumAngleSquared * In_AngleErrorWeight / (_in.ValueCount - 1);

                Out_DirectionChangeError = 0.0;
                if (In_DirectionCountTarget >= 0 && dirChangeCount != In_DirectionCountTarget)
                {
                    dirChangeCount = Math.Abs( state.DirectionCountTarget - dirChangeCount );
                    Out_DirectionChangeError = Math.Pow( In_DirectionCountPenalty, dirChangeCount );
                }
                Out_Error = Math.Sqrt( Out_ValueError + Out_ProbabilityError + Out_AngleError ) + Out_DirectionChangeError;

                _out.Error = Out_Error;
            }
        }






        public abstract class AlgorithmConfig
        {
            public int IterationCount = 0;
            public double ErrorTolerance = .40;
            public bool TerminateCalculation { get; set; } = false;
            public CoreOutputValues Best { get; internal set; }
            public IEnumerable<int> LoopUntilErrorSatisfactory()
            {
                for (IterationCount = 0; (Best == null || Best.Error > ErrorTolerance) && !TerminateCalculation; IterationCount++)
                    yield return IterationCount;
            }

            public abstract void Calculate( CoreInputValues _in, CoreOutputValues _out, CoreErrorParameters _error );
        }

        public class FloatingMutatorAlgorithm : AlgorithmConfig
        {
            public virtual double InitialStdev { get; set; } = 0.2;

            public virtual int PopulationSize { get; set; } = 300;

            public virtual int MutationCount { get; set; } = 2;

            public virtual double MultiMutateChance { get; set; } = 0.35;

            public virtual double MeanIncrementRate { get; set; } = 3;
            public virtual double MinStddevIncrementRate { get; set; } = 1.0;
            public virtual double MaxStddevIncrementRate { get; set; } = 20.0;
            public virtual double StddevIncrementRate
            { // be more exploratory when the error is high
                get
                {
                    if (Best == null) return MaxStddevIncrementRate;
                    var err = Math.Sqrt( Best.Error );
                    var retval = err.Clamp( MinStddevIncrementRate, MaxStddevIncrementRate );
                    return retval;
                }
            }
            public virtual ObjectPool<Chromosome> Pool { get; internal set; }


            public override void Calculate( CoreInputValues _in, CoreOutputValues _out, CoreErrorParameters _error )
            {
                throw new NotImplementedException();
            }
        }
    }
}
