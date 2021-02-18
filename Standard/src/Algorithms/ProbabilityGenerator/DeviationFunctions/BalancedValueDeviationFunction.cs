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
    public class BalancedValueDeviationFunction : DeviationFunction
    {
        public BalancedValueDeviationFunction() : base( VersionInfo.BalanceValueDeviationFunction ) { } // should also be treated as the "default"

        public virtual double TargetValueAcceptableDeviationPercent { get; set; } = 0.01;

        public virtual double ValueDeviationWeight { get; set; } = 1;
        public virtual double AngleDeviationWeight { get; set; } = 1;


        public override Output CalculateDeviation( Input _in, Output evalObj, DeviationDetail _detail )
        {
            int length = _in.ValueCount;

            evalObj.CalculatedValue = _in.Values.DotProduct( evalObj.Probabilities );

            double avgValue = _in.TargetValue / length;

            double sumValDevSquared = 0;
            for (int i = 0; i < length; i++)
            {
                double v = evalObj.Probabilities[i] * _in.Values[i];
                double dv = v.DifferenceAsRatioOf( avgValue );
                sumValDevSquared += dv * dv;
            }
            sumValDevSquared *= ValueDeviationWeight;


            double sumAngleSquared = 0;
            for (int i = 1; i < length; i++)
            {
                double p0 = evalObj.Probabilities[i - 1];
                double p1 = evalObj.Probabilities[i];
                double diff = p0 - p1;
                sumAngleSquared += diff * diff;
            }
            sumAngleSquared *= AngleDeviationWeight;


            var valDev = evalObj.CalculatedValue.DifferenceAsRatioOf( _in.TargetValue );
            valDev /= TargetValueAcceptableDeviationPercent;
            var valDevSquared = valDev * valDev;


            var dev = Math.Sqrt( valDevSquared + sumValDevSquared + sumAngleSquared ) / _in.ValueCount;
            evalObj.Deviation = dev;

            if (_detail != null)
            {
                var detail = (_detail as GeneralizedDeviationDetail)
                             ?? throw new ArgumentException( $"The Deviation Detail must be of type {typeof( GeneralizedDeviationDetail )}, not {_detail.GetType()}." );

                detail.Deviation = dev;
                detail.ValueDeviation = valDevSquared;
                detail.ProbabilityDeviation = sumValDevSquared;
                detail.AngleDeviation = sumAngleSquared;
            }

            return evalObj;
        }

        public override DeviationDetail NewDeviationDetailObject() => new BalanceDeviationDetail();
    }
}
