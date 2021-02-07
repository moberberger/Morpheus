using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Morpheus
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ProbabilityGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        public class Chromosome : IComparable, IComparable<Chromosome>, IEnumerable<double>, IEnumerable
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public double this[int index] => probabilities[index];

            /// <summary>
            /// 
            /// </summary>
            private PGState state;

            /// <summary>
            /// 
            /// </summary>
            private double[] probabilities;

            /// <summary>
            /// 
            /// </summary>
            public double CalculatedValue { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            public double Error { get; private set; }

            public double ValueError { get; private set; }

            public double ProbabilityError { get; private set; }

            public double AngleError { get; private set; }

            public double DirChangeError { get; private set; }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="state"></param>
            /// <param name="probabilities"></param>
            public Chromosome( PGState state = null, params double[] probabilities )
            {
                this.state = state ?? DI.Default.Get<ProbabilityGenerator>().State;

                if (probabilities == null || probabilities.Length == 0)
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

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
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
            /// <param name="index1"></param>
            /// <param name="probability1"></param>
            /// <param name="index2"></param>
            /// <param name="probability2"></param>
            internal void SetProbabilities( int index1, double probability1, int index2, double probability2 )
            {
                probabilities[index1] = probability1;
                probabilities[index2] = probability2;
                Calculate();
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
            public IEnumerator<double> GetEnumerator() => ((IEnumerable<double>)probabilities).GetEnumerator();

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            IEnumerator IEnumerable.GetEnumerator() => probabilities.GetEnumerator();

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var str = new StringBuilder( $"V: {CalculatedValue:N5}  ERR: {Error:N5}  Verr: {ValueError:N4}  Perr: {ProbabilityError:N4}  Aerr: {AngleError:N4}  Derr: {DirChangeError:N4}" );
                //for (int i = 0; i < state.ValueCount; i++)
                //    str.Append( $" {probabilities[i]:E6}" );
                return str.ToString();
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

                    var pp = (p - 1 / state.ValueCount);
                    sumProbSquared += pp * pp;

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
                ValueError = CalculatedValue.DifferenceAsRatioOf( state.TargetValue );
                ValueError /= state.TargetValueAcceptableErrorPercent;
                ValueError *= ValueError;

                // I changed this before I could compile the xfer into Morpheus. Make sure this works
                ProbabilityError = sumProbSquared * state.ProbabilityErrorWeight / state.ValueCount;
                AngleError = sumAngleSquared * state.AngleErrorWeight / (state.ValueCount - 1);

                DirChangeError = 0.0;
                if (state.DirectionCountTarget >= 0 && dirChangeCount != state.DirectionCountTarget)
                {
                    dirChangeCount = Math.Abs( state.DirectionCountTarget - dirChangeCount );
                    DirChangeError = Math.Pow( state.DirectionCountPenalty, dirChangeCount );
                }
                Error = Math.Sqrt( ValueError + ProbabilityError + AngleError ) + DirChangeError;
            }
        }
    }
}