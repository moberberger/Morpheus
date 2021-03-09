using System;
using System.Linq;
using System.Runtime.Intrinsics;

namespace Morpheus.Evolution
{
    using TintType = UInt32;

    /// <summary>
    /// A function designed to put pressure on having the probabilities brought as close to the
    /// theoretical average as possible.
    /// </summary>
    public class ProbGenDeviation
    {

        public float TargetValueAcceptableDeviationPercent = 0.01f;

        public float ProbabilitiesWeight = 1;

        public float ProbabilityErrorWeight = 1;

        public float ProbabilitiesSmoothness = 1;

        public float ValuesErrorWeight = 1;

        public float ValuesSmoothness = 1;

        public readonly ProbGenInput InputConfig;

        public ProbGenDeviation( ProbGenInput input ) => InputConfig = input ?? throw new ArgumentNullException( "input" );



        public float CalculateDeviation( ulong[] chromo )
        {
            int length = InputConfig.ValueCount;
            float expectedAverageProbability = 1.0F / length;
            float expectedAverageValue = (float)(InputConfig.TargetValue / InputConfig.ValueCount);

            Span<TintType> rawValues = new Span<ulong>( chromo ).Cast<ulong, TintType>();
            float sumRaw = 0;
            for (int i = 0; i < length; i++) sumRaw += rawValues[i];

            Span<float> probabilities = stackalloc float[InputConfig.ValueCount];
            for (int i = 0; i < length; i++) probabilities[i] = rawValues[i] / sumRaw;


            float sumValue = 0;
            float sumProbSquared = 0;
            float sumProbErrSquared = 0;
            float sumValueErrSquared = 0;

            float sumProbAngleSquared = 0;
            float sumValueAngleSquared = 0;

            float prevProb = float.NaN;
            float prevVal = float.NaN;

            for (int i = 0; i < length; i++)
            {
                float p = probabilities[i];
                float v = (float)InputConfig.Values[i];
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
                }

                prevProb = p;
                prevVal = val;
            }

            sumProbSquared /= length;
            sumProbErrSquared /= length;
            sumValueErrSquared /= length;
            sumProbAngleSquared /= length - 1;
            sumValueAngleSquared /= length - 1;

            var valDev = sumValue.DifferenceAsRatioOf( (float)InputConfig.TargetValue );
            valDev /= TargetValueAcceptableDeviationPercent;
            valDev *= valDev;

            var probDev = sumProbSquared * ProbabilitiesWeight;
            var probErrDev = sumProbErrSquared * ProbabilityErrorWeight;
            var valueErrDev = sumValueErrSquared * ValuesErrorWeight;
            var probAngleDev = sumProbAngleSquared * ProbabilitiesSmoothness;
            var valAngleDev = sumValueAngleSquared * ValuesSmoothness;

            var deviation = (float)Math.Sqrt( valDev + probDev + probErrDev + valueErrDev + probAngleDev + valAngleDev );

            return deviation;

            //if (detail != null)
            //{
            //    detail.Deviation = chromo.Deviation;
            //    detail.CalculationDeviation = valDev;

            //    detail.ProbabilitiesDeviation = probDev;
            //    detail.ProbabilitiesErrorDeviation = probErrDev;
            //    detail.ProbabilitiesSmoothnessDeviation = probAngleDev;
            //    detail.ValuesErrorDeviation = valueErrDev;
            //    detail.ValuesSmoothnessDeviation = valAngleDev;

            //    detail.DirectionChangeCount = dirChangeCount;
            //    detail.DirectionChangeDeviation = dirChangeError;

            //    detail.TargetValue = InputConfig.TargetValue;
            //    detail.CalculatedValue = chromo.CalculatedValue;
            //    Array.Copy( InputConfig.Values, detail.Values, length );
            //    Array.Copy( chromo.RawProbabilities, detail.Probabilities, length );
            //}
        }
    }
}
