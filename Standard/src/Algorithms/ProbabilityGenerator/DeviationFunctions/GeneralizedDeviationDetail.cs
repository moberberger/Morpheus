using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.ProbabilityGeneratorNS
{
    public class GeneralizedDeviationDetail : DeviationDetail
    {
        public GeneralizedDeviationDetail() : base( VersionInfo.GeneralizedDeviationDetail ) { }

        public double ValueDeviation;
        public double ProbabilityDeviation;
        public double AngleDeviation;
        public double DirectionChangeDeviation;

        public override string ToString() => $"{Deviation:N4}  v:{ValueDeviation:N4}  p:{ProbabilityDeviation:N4}  a:{AngleDeviation:N4}  dc:{DirectionChangeDeviation:N4}";
    }
}
