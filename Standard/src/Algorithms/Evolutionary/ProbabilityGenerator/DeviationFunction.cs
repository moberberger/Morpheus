using System;

namespace Morpheus.Evolution.PGNS
{
    /// <summary>
    /// A function designed to put pressure on having the probabilities brought as close to the
    /// theoretical average as possible.
    /// </summary>
    public class DeviationFunction
    {
        public float TargetValueAcceptableDeviationPercent = 0.01f;

        public float ProbabilitiesWeight = 1;

        public float ProbabilityErrorWeight = 1;

        public float ProbabilitiesSmoothness = 1;

        public float ValuesErrorWeight = 1;

        public float ValuesSmoothness = 1;

        public int TargetDirectionChangeCount = -1;

        public float DirectionChangeMagnitude = 10;


        public Chromosome CalculateDeviation( Config config, Chromosome chromo, DeviationDetail detail )
        {
            int length = config.ValueCount;
            float expectedAverageProbability = 1.0F / length;
            float expectedAverageValue = (float)(config.TargetValue / config.ValueCount);

            double _sumRawProb = 0;
            for (int i = 0; i < length; i++)
                _sumRawProb += chromo.RawProbabilities[i];
            float sumRawProb = (float)_sumRawProb; // cast once, tight loop

            float sumValue = 0;
            float sumProbSquared = 0;
            float sumProbErrSquared = 0;
            float sumValueErrSquared = 0;

            float sumProbAngleSquared = 0;
            float sumValueAngleSquared = 0;
            int dirChangeCount = 0;

            float prevProb = float.NaN;
            float prevVal = float.NaN;
            bool dirChangeEnabled = TargetDirectionChangeCount >= 0;

            for (int i = 0; i < length; i++)
            {
                float p = (float)chromo.RawProbabilities[i] / sumRawProb;
                float v = (float)config.Values[i];
                float val = p * v;
                float diffProb = p.DifferenceAsRatioOf( expectedAverageProbability );
                float diffValue = val.DifferenceAsRatioOf( expectedAverageValue );

                var p_2 = p * p;
                var diffProb_2 = diffProb * diffProb;
                var diffVal_2 = diffValue * diffValue;

                sumValue += val;
                sumProbSquared += p_2;
                sumProbErrSquared += diffProb_2;
                sumValueErrSquared += diffVal_2;

                if (i > 0)
                {
                    float diffP = p.DifferenceAsRatioOf( prevProb );
                    sumProbAngleSquared += diffP * diffP;

                    float diffV = val.DifferenceAsRatioOf( prevVal );
                    sumValueAngleSquared += diffV * diffV;

                    if (dirChangeEnabled && i < length - 1)
                    {
                        bool prev = p > prevProb;
                        bool next = chromo.RawProbabilities[i + 1] > p;

                        if (prev != next)
                            dirChangeCount++;
                    }
                }

                prevProb = p;
                prevVal = val;
            }
            chromo.CalculatedValue = sumValue;

            sumProbSquared /= length;
            sumProbErrSquared /= length;
            sumValueErrSquared /= length;
            sumProbAngleSquared /= length - 1;
            sumValueAngleSquared /= length - 1;

            float dirChangeError = (float)(
                (dirChangeEnabled && dirChangeCount != TargetDirectionChangeCount)
                ? Math.Pow( DirectionChangeMagnitude, Math.Abs( dirChangeCount - TargetDirectionChangeCount ) )
                : 0);


            var valDev = sumValue.DifferenceAsRatioOf( (float)config.TargetValue );
            valDev /= TargetValueAcceptableDeviationPercent;
            valDev *= valDev;

            var probDev = sumProbSquared * ProbabilitiesWeight;
            var probErrDev = sumProbErrSquared * ProbabilityErrorWeight;
            var valueErrDev = sumValueErrSquared * ValuesErrorWeight;
            var probAngleDev = sumProbAngleSquared * ProbabilitiesSmoothness;
            var valAngleDev = sumValueAngleSquared * ValuesSmoothness;

            chromo.Deviation = (float)Math.Sqrt( valDev + probDev + probErrDev + valueErrDev + probAngleDev + valAngleDev ) + dirChangeError;

            if (detail != null)
            {
                detail.Deviation = chromo.Deviation;
                detail.CalculationDeviation = valDev;

                detail.ProbabilitiesDeviation = probDev;
                detail.ProbabilitiesErrorDeviation = probErrDev;
                detail.ProbabilitiesSmoothnessDeviation = probAngleDev;
                detail.ValuesErrorDeviation = valueErrDev;
                detail.ValuesSmoothnessDeviation = valAngleDev;

                detail.DirectionChangeCount = dirChangeCount;
                detail.DirectionChangeDeviation = dirChangeError;

                detail.TargetValue = config.TargetValue;
                detail.CalculatedValue = chromo.CalculatedValue;
                Array.Copy( config.Values, detail.Values, length );
                Array.Copy( chromo.RawProbabilities, detail.Probabilities, length );
            }

            return chromo;
        }
    }
}
