namespace Morpheus.Evolution.PGNS
{
    using PGEngine = Engine<Evolution.PGNS.Chromosome, Config, DeviationDetail>;

    public class DeviationDetail
    {
        private PGEngine Engine;
        public DeviationDetail( PGEngine engine = null ) : this( engine.InputConfig.ValueCount ) => Engine = engine;

        public float Deviation;

        public float CalculationDeviation;
        public float ProbabilitiesDeviation;
        public float ProbabilitiesErrorDeviation;
        public float ProbabilitiesSmoothnessDeviation;

        public float ValuesErrorDeviation;
        public float ValuesSmoothnessDeviation;

        public int DirectionChangeCount;
        public float DirectionChangeDeviation;

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


        public override string ToString()
        {
            if (Engine != null)
                Engine.DeviationFunction( Engine.InputConfig, Engine.Best, this );

            return $"dev:{Deviation:N4}  v:{CalculationDeviation:N4}  p:{ProbabilitiesDeviation:N4}  pe:{ProbabilitiesErrorDeviation:N4}  ps:{ProbabilitiesSmoothnessDeviation:N4}" +
            $"  ve:{ValuesErrorDeviation:N4}  vs:{ValuesSmoothnessDeviation:N4}  dc:{DirectionChangeDeviation:N4}";
        }
    }
}
