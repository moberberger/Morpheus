using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.ProbabilityGeneratorNS
{
    public class DeviationDetail
    {
        public double Deviation;
        public double ValueDeviation;
        public double ProbabilityDeviation;
        public double AngleDeviation;
        public double ValuesDeviation;
        public double DirectionChangeDeviation;

        public override string ToString() => $"dev:{Deviation:N4}  v:{ValueDeviation:N4}  p:{ProbabilityDeviation:N4}  a:{AngleDeviation:N4}  vals:{ValuesDeviation:N4}  dc:{DirectionChangeDeviation:N4}";
    }
}
