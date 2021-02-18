using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.ProbabilityGeneratorNS
{
    public class BalanceDeviationDetail : DeviationDetail
    {
        public BalanceDeviationDetail() : base( VersionInfo.BalanceDeviationDetail ) { }

        public double ValueDeviation;
        public double ValuesDeviation;
        public double AnglesDeviation;

        public override string ToString() => $"{Deviation:N4}  v:{ValueDeviation:N4}  p:{ProbabilityDeviation:N4}  a:{AngleDeviation:N4}  dc:{DirectionChangeDeviation:N4}";
    }
}
