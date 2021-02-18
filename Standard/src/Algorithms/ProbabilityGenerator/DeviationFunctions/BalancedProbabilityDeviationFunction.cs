using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// A generalized deviation function which is meant to create as "smooth" of a curve as
    /// possible
    /// </summary>
    public class BalancedProbabilityDeviationFunction : DeviationFunction
    {
        public BalancedProbabilityDeviationFunction() : base( VersionInfo.BalanceProbabilityDeviationFunction ) { } // should also be treated as the "default"

        public virtual double TargetValueAcceptableDeviationPercent { get; set; } = 0.01;

        public virtual double ProbabilityDeviationWeight { get; set; } = 1;

        public virtual double AngleDeviationWeight { get; set; } = 1;

        public virtual double DirectionChangePenalty { get; set; } = 10.0;
        public virtual int DirectionChangeTarget { get; set; } = -1;



        public override Output CalculateDeviation( Input _in, Output evalObj, DeviationDetail _detail )
        {
            int length = _in.ValueCount;

            evalObj.CalculatedValue = _in.Values.DotProduct( evalObj.Probabilities );

            double sumProbSquared = 0;
            for (int i = 0; i < length; i++)
            {
                double p = evalObj.Probabilities[i];
                sumProbSquared += p * p;
            }

            double sumAngleSquared = 0;
            for (int i = 1; i < length; i++)
            {
                double p0 = evalObj.Probabilities[i - 1];
                double p1 = evalObj.Probabilities[i];
                double diff = p0 - p1;
                sumAngleSquared += diff * diff;
            }

            int dirChangeCount = 0;
            for (int i = 1; i < length - 1; i++)
            {
                var p = evalObj.Probabilities[i];
                if (Math.Sign( p - evalObj.Probabilities[i - 1] ) != Math.Sign( evalObj.Probabilities[i + 1] - p ))
                    dirChangeCount++;
            }

            var valDev = evalObj.CalculatedValue.DifferenceAsRatioOf( _in.TargetValue );
            valDev /= TargetValueAcceptableDeviationPercent;
            valDev *= valDev;

            var probDev = sumProbSquared * ProbabilityDeviationWeight;

            var angleDev = sumAngleSquared * AngleDeviationWeight;

            var dirChgDev = 0.0;
            if (DirectionChangeTarget >= 0 && dirChangeCount != DirectionChangeTarget)
            {
                dirChangeCount = Math.Abs( DirectionChangeTarget - dirChangeCount );
                dirChgDev = Math.Pow( DirectionChangePenalty, dirChangeCount );
            }

            var dev = Math.Sqrt( valDev + probDev + angleDev ) / _in.ValueCount + dirChgDev;
            evalObj.Deviation = dev;

            if (_detail != null)
            {
                var detail = (_detail as GeneralizedDeviationDetail)
                             ?? throw new ArgumentException( $"The Deviation Detail must be of type {typeof( GeneralizedDeviationDetail )}, not {_detail.GetType()}." );

                detail.Deviation = dev;
                detail.ValueDeviation = valDev;
                detail.ProbabilityDeviation = probDev;
                detail.AngleDeviation = angleDev;
                detail.DirectionChangeDeviation = dirChgDev;
            }

            return evalObj;
        }

        public override DeviationDetail NewDeviationDetailObject() => new GeneralizedDeviationDetail();
    }
}
