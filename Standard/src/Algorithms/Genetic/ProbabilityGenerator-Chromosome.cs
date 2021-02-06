using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

using State = Morpheus.ProbabilityGenerator.State;

namespace Morpheus
{
    partial class ProbabilityGenerator
    {
        public class Chromosome : IComparable, IComparable<Chromosome>, IEnumerable<double>, IEnumerable
        {
            public double this[int index] => probabilities[index];

            private ProbabilityGenerator.State state;

            private double[] probabilities;

            public double CalculatedValue { get; private set; }

            public double Error { get; private set; }



            public Chromosome( State state = null, params double[] probabilities )
            {
                state = state ?? DI.Default.Get<State>();

                if (probabilities == null)
                {
                    var probArray = Lib.Repeat( state.ValueCount, () => Math.Abs( Rng.Default.NextGaussian( 0, state.InitialStdev ) ) )
                                       .OrderByDescending( x => x )
                                       .ToArray();

                    var values = Lib.ForLoop( state.ValueCount )
                                    .Select( _idx => new { index = _idx, diff = Math.Abs( state.Values[_idx] - state.TargetValue ) } )
                                    .OrderBy( _x => _x.diff )
                                    .ToList();

                    this.probabilities = values.Select( validx => probArray[validx.index] ).ToArray();
                }
                else
                {
                    this.probabilities = probabilities;
                }

                Calculate();
            }

            public Chromosome Mutate()
            {
                var x = state.Pool.Get();
                x.state = state;
                Array.Copy( probabilities, x.probabilities, state.ValueCount );

                do
                {
                    int idx = Rng.Default.Next( state.ValueCount );

                    var factor = Math.Abs( (double)Rng.Default.NextGaussian( state.MeanIncrementRate, state.StddevIncrementRate ) );
                    var newVal = x.probabilities[idx] * factor;
                    newVal = Math.Max( newVal, state.MinimumProbability );

                    x.probabilities[idx] = newVal;

                } while (Rng.Default.NextDouble() < state.MultiMutateChance);

                x.Calculate();
                return x;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo( Chromosome other )
            {
                if (other == null) throw new ArgumentNullException();

                // if (double.IsNaN( Error ) || double.IsNaN( other.Error )) return 0; if
                // (double.IsInfinity( Error ) || double.IsInfinity( other.Error )) return 0;

                return Math.Sign( Error - other.Error );
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            int IComparable.CompareTo( object obj ) => CompareTo( obj as Chromosome );

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var str = new StringBuilder( $"E: {Error:N5} V: {CalculatedValue:N3} " );
                for (int i = 0; i < state.ValueCount; i++)
                    str.Append( $" {probabilities[i]:E6}" );
                return str.ToString();
            }

            public IEnumerator<double> GetEnumerator()
            {
                return (IEnumerator<double>)probabilities.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return probabilities.GetEnumerator();
            }









            /// <summary>
            /// ****************************************************************************************
            /// ****************************************************************************************
            /// ****************************************************************************************
            /// ****************************************************************************************
            /// </summary>
            private void Calculate()
            {
                double sumProb = 0;
                double sumProbSquared = 0;
                double sumValue = 0;
                double sumAngleSquared = 0;
                int dirChangeCount = 0;

                for (int i = 0; i < state.ValueCount; i++)
                    sumProb += probabilities[i]; // used to normalize probabilities (0..1)

                for (int i = 0; i < state.ValueCount; i++)
                {
                    probabilities[i] /= sumProb;
                    var p = probabilities[i];

                    sumProbSquared += p * p;
                    sumValue += p * state.Values[i];
                    if (i > 0)
                    {
                        var angle = p - probabilities[i - 1];
                        sumAngleSquared += angle * angle;

                        if (i < state.ValueCount - 1 && Math.Sign( p - probabilities[i - 1] ) != Math.Sign( probabilities[i + 1] - p ))
                            dirChangeCount++;
                    }
                }
                CalculatedValue = sumValue;

                // ERROR CALCULATION HERE Ratio- ideally this is zero, but is 1 when the target is
                // at the "Acceptable Error"
                var valErr = CalculatedValue.DifferenceAsRatioOf( state.TargetValue );
                valErr /= state.TargetValueAcceptableErrorPercent;
                valErr *= valErr;

                // I changed this before I could compile the xfer into Morpheus. Make sure this works
                var probErr = sumProbSquared * state.ProbabilityErrorWeight;
                var angleErr = sumAngleSquared * state.AngleErrorWeight;

                var dirChangeErr = 0.0;
                if (dirChangeCount != state.DirectionCountTarget)
                {
                    dirChangeCount = Math.Abs( state.DirectionCountTarget - dirChangeCount );
                    dirChangeErr = Math.Pow( state.DirectionCountPenalty, dirChangeCount );
                }
                Error = Math.Sqrt( valErr + probErr + angleErr ) + dirChangeErr;
            }

            internal void SetProbabilities( int lowIdx, double probability1, int highIdx, double probability2 )
            {
                probabilities[lowIdx] = probability1;
                probabilities[highIdx] = probability2;
                Calculate();
            }
        }
    }
}