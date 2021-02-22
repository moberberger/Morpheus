namespace Morpheus.Evolution.PGNS
{
    public class DeviationDetail
    {
        public double Deviation;

        public double CalculationDeviation;
        public double ProbabilitiesDeviation;
        public double ProbabilitiesErrorDeviation;
        public double ProbabilitiesSmoothnessDeviation;

        public double ValuesErrorDeviation;
        public double ValuesSmoothnessDeviation;

        public int DirectionChangeCount;
        public double DirectionChangeDeviation;

        public double TargetValue;
        public double CalculatedValue;
        public double[] Values;
        public double[] Probabilities;

        /// <summary>
        /// Must be constructed to match the Input configuration's size
        /// </summary>
        /// <param name="size"></param>
        public DeviationDetail( int size )
        {
            Values = new double[size];
            Probabilities = new double[size];
        }


        public override string ToString() => $"dev:{Deviation:N4}  v:{CalculationDeviation:N4}  p:{ProbabilitiesDeviation:N4}  pe:{ProbabilitiesErrorDeviation:N4}  a:{ProbabilitiesSmoothnessDeviation:N4}" +
            $"  ve:{ValuesErrorDeviation:N4}  vs:{ValuesSmoothnessDeviation}  dc:{DirectionChangeDeviation:N4}";
    }
}
