using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace Morpheus.ProbabilityGeneratorNS
{
    public sealed class Engine<TConfig, TChromosome>
        where TConfig : Config
        where TChromosome : Chromosome
    {
        private readonly LCPRNG _rng = new LCPRNG_MMIX();

        public int PopulationSize { get; set; } = 150;

        public double AcceptableDeviation { get; set; } = 0.25;

        public bool TerminateGeneration { private get; set; }

        public int IterationCount { get; private set; }


        public TChromosome Best { get; private set; }

        public TChromosome[] SampleSet { get; private set; }

        public TChromosome[] ResultSet { get; private set; }

        private double[] _sampleSums;

        private double _sampleSetSumDeviations => _sampleSums[PopulationSize - 1];




        public TChromosome Generate( Config input, Action<Config, TChromosome, DeviationDetail> deviationFn, Action<Func<TChromosome>, TChromosome> evolver )
        {
            SampleSet = new TChromosome[PopulationSize];
            ResultSet = new TChromosome[PopulationSize];
            _sampleSums = new double[PopulationSize];

            Initialize( input, deviationFn );

            Iterate( input, deviationFn, evolver );

            return Best;
        }


        void Initialize( Config input, Action<Config, TChromosome, DeviationDetail> deviationFn )
        {
            for (int i = 0; i < PopulationSize; i++)
            {
                var chromo = new TChromosome( input.ValueCount );
                deviationFn( input, chromo, null );
                SampleSet[i] = chromo;
                ResultSet[i] = new TChromosome( input );
            }

            ProcessSampleSet();
        }


        void Iterate( Config input, Action<Config, TChromosome, DeviationDetail> deviationFn, Action<Func<TChromosome>, TChromosome> evolver )
        {
            TerminateGeneration = false;
            for (IterationCount = 0; Best.Deviation < AcceptableDeviation && !TerminateGeneration; IterationCount++)
            {
                // Elitism
                ResultSet[0].CopyFrom( Best );

                // Generation
                for (int i = 1; i < PopulationSize; i++) // 1 because of Elitism
                { // can be parallel
                    var output = ResultSet[i];
                    evolver( Sample, output );
                    deviationFn( input, output, null );
                }

                // Transfer back
                for (int i = 0; i < PopulationSize; i++)
                    Lib.Swap( ref ResultSet[i], ref SampleSet[i] );

                ProcessSampleSet();
            }
        }



        void ProcessSampleSet()
        {
            double sumInverses = 0;
            for (int i = 0; i < PopulationSize; i++)
                sumInverses += 1.0 / SampleSet[i].Deviation;

            double sum = 0;
            double bestDeviation = double.MaxValue;
            for (int i = 0; i < PopulationSize; i++)
            {
                var chromo = SampleSet[i];
                sum += 1.0 / (sumInverses * chromo.Deviation);
                _sampleSums[i] = sum;

                if (chromo.Deviation < bestDeviation)
                {
                    bestDeviation = chromo.Deviation;
                    Best = chromo;
                }
            }
        }



        public TChromosome Sample()
        {
            int low = 0;
            int high = PopulationSize;
            int mid = 0;

            var selection = _rng.NextDouble() * _sampleSetSumDeviations;

            while (low < high)
            {
                mid = low + (high - low) / 2;

                if (selection > _sampleSums[mid])
                    low = mid + 1;
                else
                    high = mid - 1;
            }
            if (low == high) mid = low;

            return SampleSet[mid];
        }

    }
}
