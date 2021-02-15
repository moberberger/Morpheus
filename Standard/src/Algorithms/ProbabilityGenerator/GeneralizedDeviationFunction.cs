using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// A generalized deviation function which is meant to create as "smooth" of a curve as
    /// possible
    /// </summary>
    public class GeneralizedDeviationFunction : DeviationFunction
    {
        public GeneralizedDeviationFunction() : base( VersionInfo.GeneralizedDeviationFunction ) { } // should also be treated as the "default"

        public virtual double TargetValueAcceptableDeviationPercent { get; set; } = 0.05;

        public virtual double ProbabilityDeviationWeight { get; set; } = 10;

        public virtual double AngleDeviationWeight { get; set; } = 1000;

        public virtual double DirectionChangePenalty { get; set; } = 10.0;
        public virtual int DirectionChangeTarget { get; set; } = 1;





        public override Output CalculateDeviation( Input _in, Output evalObj, DeviationDetail _detail )
        {
            double sumProb = 0;
            double sumProbSquared = 0;
            double sumValue = 0;
            double sumAngleSquared = 0;
            int dirChangeCount = 0;

            for (int i = 0; i < _in.ValueCount; i++)
                sumProb += evalObj.Probabilities[i]; // used to normalize probabilities (0..1)

            for (int i = 0; i < _in.ValueCount; i++)
            {
                evalObj.Probabilities[i] /= sumProb;
                var p = evalObj.Probabilities[i];

                var pp = (p - 1 / _in.ValueCount); // 1/ValueCount is the average probability
                sumProbSquared += pp * pp;

                sumValue += p * _in.Values[i];

                if (i > 0)
                {
                    var angle = p - evalObj.Probabilities[i - 1];
                    sumAngleSquared += angle * angle;

                    if (i < _in.ValueCount - 1 && Math.Sign( p - evalObj.Probabilities[i - 1] ) != Math.Sign( evalObj.Probabilities[i + 1] - p ))
                        dirChangeCount++;
                }
            }

            evalObj.CalculatedValue = sumValue;

            // Deviation and DeviationDetails generated here
            var valDev = sumValue.DifferenceAsRatioOf( _in.TargetValue );
            valDev /= TargetValueAcceptableDeviationPercent;
            valDev *= valDev;

            // I changed this before I could compile the xfer into Morpheus. Make sure this
            // works
            var probDev = sumProbSquared * ProbabilityDeviationWeight / _in.ValueCount;
            var angleDev = sumAngleSquared * AngleDeviationWeight / (_in.ValueCount - 1);

            var dirChgDev = 0.0;
            if (DirectionChangeTarget >= 0 && dirChangeCount != DirectionChangeTarget)
            {
                dirChangeCount = Math.Abs( DirectionChangeTarget - dirChangeCount );
                dirChgDev = Math.Pow( DirectionChangePenalty, dirChangeCount );
            }

            var dev = Math.Sqrt( valDev + probDev + angleDev ) + dirChgDev;
            evalObj.Deviation = dev;

            if (_detail != null)
            {
                var detail = (_detail as GeneralizedDeviationDetail)
                             ?? throw new ArgumentException( $"The Deviation Detail must be of type {typeof( GeneralizedDeviationDetail )}, not {_detail.GetType()}." );

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
