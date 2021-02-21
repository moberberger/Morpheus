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

        public override string ToString() => $"{Deviation:N4}  v:{ValueDeviation:N4}  pv:{ValuesDeviation:N4}  a:{AnglesDeviation:N4}";
    }
}
