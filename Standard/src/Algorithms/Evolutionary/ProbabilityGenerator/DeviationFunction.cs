using System;

namespace Morpheus.Evolution.PGNS
{
    /// <summary>
    /// A function designed to put pressure on having the probabilities brought as close to the
    /// theoretical average as possible.
    /// </summary>
    public class DeviationFunction
    {
        public double TargetValueAcceptableDeviationPercent = 0.01;

        public double ProbabilitiesWeight = 1;

        public double ProbabilitErrorWeight = 1;

        public double ProbabilitiesSmoothness = 1;

        public double ValuesErrorWeight = 1;

        public double ValuesSmoothness = 1;

        public int TargetDirectionChangeCount = -1;

        public double DirectionChangeMagnitude = 10;


        public Chromosome CalculateDeviation( Config config, Chromosome chromo, DeviationDetail detail )
        {
            int length = config.ValueCount;
            double expectedAverageValue = config.TargetValue / config.ValueCount;

            double sumValue = 0.0;
            double sumProbSquared = 0;
            double sumProbErrSquared = 0;
            double sumValueErrSquared = 0;

            double sumProbAngleSquared = 0;
            double sumValueAngleSquared = 0;
            int dirChangeCount = 0;

            double prevProb = double.NaN;
            double prevVal = double.NaN;

            for (int i = 0; i < length; i++)
            {
                double p = chromo.Probabilities[i];
                double v = config.Values[i];
                double val = p * v;
                double diffProb = p - 1 / length;
                double diffValue = v.DifferenceAsRatioOf( expectedAverageValue );


                sumValue += val;
                sumProbSquared += p * p;
                sumProbErrSquared += diffProb * diffProb;
                sumValueErrSquared += diffValue * diffValue;

                if (i > 0)
                {
                    double diffP = p - prevProb;
                    sumProbAngleSquared += diffP * diffP;

                    double diffV = val - prevVal;
                    sumValueAngleSquared += diffV * diffV;

                    if (i < length - 1
                        && TargetDirectionChangeCount >= 0
                        && Math.Sign( p - chromo.Probabilities[i - 1] ) != Math.Sign( chromo.Probabilities[i + 1] - p ))
                    {
                        dirChangeCount++;
                    }
                }

                prevProb = p;
                prevVal = val;
            }
            chromo.CalculatedValue = sumValue;

            sumProbSquared /= (length - 1).ButNotLessThan( 1 );
            sumProbErrSquared /= (length - 1).ButNotLessThan( 1 );
            sumValueErrSquared /= (length - 1).ButNotLessThan( 1 );
            sumProbAngleSquared /= (length - 2).ButNotLessThan( 1 );
            sumValueAngleSquared /= (length - 2).ButNotLessThan( 1 );

            double dirChangeError =
                (TargetDirectionChangeCount < 0 || dirChangeCount == TargetDirectionChangeCount)
                ? 0.0
                : Math.Pow( DirectionChangeMagnitude, Math.Abs( dirChangeCount - TargetDirectionChangeCount ) );


            var valDev = sumValue.DifferenceAsRatioOf( config.TargetValue );
            valDev /= TargetValueAcceptableDeviationPercent;
            valDev *= valDev;

            var probDev = sumProbSquared * ProbabilitiesWeight;
            var probErrDev = sumProbErrSquared * ProbabilitErrorWeight;
            var valueErrDev = sumValueErrSquared * ValuesErrorWeight;
            var probAngleDev = sumProbAngleSquared * ProbabilitiesSmoothness;
            var valAngleDev = sumValueAngleSquared * ValuesSmoothness;

            chromo.Deviation = valDev + probDev + probErrDev + valueErrDev + probAngleDev + valAngleDev + dirChangeError;

            if (detail != null)
            {
                detail.Deviation = chromo.Deviation;
                detail.CalculationDeviation = valDev;

                detail.ProbabilitiesDeviation = probDev;
                detail.ProbabilitiesErrorDeviation = probErrDev;
                detail.ProbabilitiesSmoothnessDeviation = probAngleDev;
                detail.ValuesErrorDeviation = valueErrDev;
                detail.ValuesSmoothnessDeviation = valAngleDev;

                detail.TargetValue = config.TargetValue;
                detail.CalculatedValue = chromo.CalculatedValue;
                Array.Copy( config.Values, detail.Values, length );
                Array.Copy( chromo.Probabilities, detail.Probabilities, length );
            }

            return chromo;
        }
    }
}
