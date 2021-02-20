using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace Morpheus.ProbabilityGeneratorNS
{
    public class Engine
    {
        public class SampleEntry : IComparable, IComparable<SampleEntry>
        {
            public double SampleSum;
            public Output Chromosome;

            public SampleEntry( double sum, Output chromo )
            {
                this.SampleSum = sum;
                this.Chromosome = chromo;
            }

            public int CompareTo( SampleEntry other )
            {
                if (other == null) throw new ArgumentNullException( "Not allowed to compare to NULL" );
                return Math.Sign( SampleSum - other.SampleSum );
            }

            int IComparable.CompareTo( object obj )
            {
                return CompareTo( obj as SampleEntry );
            }
        }


        public int PopulationSize { get; set; } = 150;

        public double AcceptableDeviation { get; set; } = 0.25;

        public bool TerminateGeneration { get; set; }

        public int IterationCount { get; private set; }

        public Output Best { get; protected set; }


        public SampleEntry[] SampleSet { get; private set; }

        public Output[] ResultSet { get; private set; }

        private double sampleSetSumDeviations;


        public Output Generate( Input input, Action<Input, Output, DeviationDetail> deviationFn, Action<Func<Output>, Output> mutator )
        {
            SampleSet = new SampleEntry[PopulationSize];
            ResultSet = new Output[PopulationSize];

            double sum = 0;
            double bestDeviation = double.MaxValue;

            for (int i = 0; i < PopulationSize; i++)
            {
                ResultSet[i] = new Output( input );

                var chromo = new Output( input );

                deviationFn( input, chromo, null );

                sum += chromo.Deviation;

                var se = new SampleEntry( sum, chromo );

                SampleSet[i] = se;

                if (chromo.Deviation < bestDeviation)
                {
                    bestDeviation = chromo.Deviation;
                    Best = chromo;
                }
            }
            sampleSetSumDeviations = sum;

            for (IterationCount = 0; Best.Deviation < AcceptableDeviation && !TerminateGeneration; IterationCount++)
            {
                // Elitism
                ResultSet[0].CopyFrom( Best );

                // Generation
                for (int i = 1; i < PopulationSize; i++) // 1 because of Elitism
                {
                    var output = ResultSet[i];
                    mutator( Sample, output );
                    deviationFn( input, output, null );
                }

                // Transfer back
                sum = 0;
                bestDeviation = double.MaxValue;
                for (int i = 0; i < PopulationSize; i++)
                {
                    var chromo = ResultSet[i];
                    ResultSet[i] = SampleSet[i].Chromosome;
                    SampleSet[i].Chromosome = chromo;

                    sum += chromo.Deviation;
                    SampleSet[i].SampleSum = sum;

                    if (chromo.Deviation < bestDeviation)
                    {
                        bestDeviation = chromo.Deviation;
                        Best = chromo;
                    }
                }
                sampleSetSumDeviations = sum;
            }

            return Best;
        }

        internal Output Sample()
        {
            int low = 0;
            int high = PopulationSize;
            int mid = 0;

            var selection = DI.Default.Get<Random>().NextDouble() * sampleSetSumDeviations;

            while (low < high)
            {
                mid = low + (high - low) / 2;
                var se = SampleSet[mid];

                if (selection > se.SampleSum)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return SampleSet[mid].Chromosome;
        }
    }
}
