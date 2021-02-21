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
    public class PressureProbabilitiesDeviation
    {
        public virtual double TargetValueAcceptableDeviationPercent { get; set; } = 0.01;

        public virtual double ProbabilityDeviationWeight { get; set; } = 1;

        public virtual double AngleDeviationWeight { get; set; } = 1;


        public override Chromosome CalculateDeviation( Config _config, Chromosome _chromo, DeviationDetail _detail )
        {
            var config = _config as ProbabilityGeneratorConfig;
            var chromo = _chromo as Chromosome;
            int length = config.ValueCount;

            chromo.CalculatedValue = config.Values.DotProduct( chromo.Probabilities );

            double sumProbSquared = 0;
            for (int i = 0; i < length; i++)
            {
                double p = chromo.Probabilities[i];
                sumProbSquared += p * p;
            }

            double sumAngleSquared = 0;
            for (int i = 1; i < length; i++)
            {
                double p0 = chromo.Probabilities[i - 1];
                double p1 = chromo.Probabilities[i];
                double diff = p0 - p1;
                sumAngleSquared += diff * diff;
            }

            int dirChangeCount = 0;
            for (int i = 1; i < length - 1; i++)
            {
                var p = chromo.Probabilities[i];
                if (Math.Sign( p - chromo.Probabilities[i - 1] ) != Math.Sign( chromo.Probabilities[i + 1] - p ))
                    dirChangeCount++;
            }

            var valDev = chromo.CalculatedValue.DifferenceAsRatioOf( config.TargetValue );
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

            var dev = Math.Sqrt( valDev + probDev + angleDev ) / config.ValueCount + dirChgDev;
            chromo.Deviation = dev;

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

            return chromo;
        }

        public override DeviationDetail NewDeviationDetailObject() => new GeneralizedDeviationDetail();
    }
}
