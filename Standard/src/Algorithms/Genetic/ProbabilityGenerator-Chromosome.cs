using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

namespace Morpheus.PGNS
{
    public class Chromosome : IComparable, IComparable<Chromosome>, IEnumerable<double>, IEnumerable
    {
        public double this[int index] => probabilities[index];

        private Config config;

        private double[] probabilities;

        public double CalculatedValue { get; private set; }

        public double Error { get; private set; }



        public Chromosome( Config config = null, params double[] probabilities )
        {
            config = config ?? DI.Default.Get<Config>();

            if (probabilities == null)
            {
                var probArray = Lib.Repeat( config.ValueCount, () => Math.Abs( Rng.Default.NextGaussian( 0, config.InitialStdev ) ) )
                                   .OrderByDescending( x => x )
                                   .ToArray();

                var values = Lib.ForLoop( config.ValueCount )
                                .Select( _idx => new { index = _idx, diff = Math.Abs( config.Values[_idx] - config.TargetValue ) } )
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
            var x = config.Pool.Get();
            x.config = config;
            Array.Copy( probabilities, x.probabilities, config.ValueCount );

            do
            {
                int idx = Rng.Default.Next( config.ValueCount );

                var factor = Math.Abs( (double)Rng.Default.NextGaussian( config.MeanIncrementRate, config.StddevIncrementRate ) );
                var newVal = x.probabilities[idx] * factor;
                newVal = Math.Max( newVal, config.MinimumProbability );

                x.probabilities[idx] = newVal;

            } while (Rng.Default.NextDouble() < config.MultiMutateChance);

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
            for (int i = 0; i < config.ValueCount; i++)
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

            for (int i = 0; i < config.ValueCount; i++)
                sumProb += probabilities[i]; // used to normalize probabilities (0..1)

            for (int i = 0; i < config.ValueCount; i++)
            {
                probabilities[i] /= sumProb;
                var p = probabilities[i];

                sumProbSquared += p * p;
                sumValue += p * config.Values[i];
                if (i > 0)
                {
                    var angle = p - probabilities[i - 1];
                    sumAngleSquared += angle * angle;

                    if (i < config.ValueCount - 1 && Math.Sign( p - probabilities[i - 1] ) != Math.Sign( probabilities[i + 1] - p ))
                        dirChangeCount++;
                }
            }
            CalculatedValue = sumValue;

            // ERROR CALCULATION HERE Ratio- ideally this is zero, but is 1 when the target is
            // at the "Acceptable Error"
            var valErr = CalculatedValue.DifferenceAsRatioOf( config.TargetValue );
            valErr /= config.TargetValueAcceptableErrorPercent;
            valErr *= valErr;

            // I changed this before I could compile the xfer into Morpheus. Make sure this works
            var probErr = sumProbSquared * config.ProbabilityErrorWeight;
            var angleErr = sumAngleSquared * config.AngleErrorWeight;

            var dirChangeErr = 0.0;
            if (dirChangeCount != config.DirectionCountTarget)
            {
                dirChangeCount = Math.Abs( config.DirectionCountTarget - dirChangeCount );
                dirChangeErr = Math.Pow( config.DirectionCountPenalty, dirChangeCount );
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
