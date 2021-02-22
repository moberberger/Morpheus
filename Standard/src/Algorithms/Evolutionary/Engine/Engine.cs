using System;

namespace Morpheus.Evolution
{
    public class Engine<TChromosome, TInputType, TDeviationDetailType>
        where TChromosome : Chromosome
    {
        public delegate TChromosome FnDeviationCalculator( TInputType input, TChromosome chromo, TDeviationDetailType detail );
        public delegate void FnEvolver( Func<TChromosome> sampler, TChromosome evolveInto );
        public delegate TChromosome FnChromosomeCreator( TInputType input, bool initialized );



        private readonly LCPRNG _rng = new LCPRNG_MMIX();

        private double[] _sampleSums;

        private double _sampleSetSumDeviations => _sampleSums[PopulationSize - 1];



        public bool UseElitism { get; set; } = true;

        /// <summary>
        /// Marginally most efficient as an exponent of 2
        /// </summary>
        public readonly int PopulationSize;

        public TInputType InputConfig { get; set; }

        public FnDeviationCalculator DeviationFunction { get; set; }

        public FnEvolver Evolver { get; set; }

        public FnChromosomeCreator ChromosomeCreator { get; set; }



        public int IterationCount { get; private set; }

        public TChromosome Best { get; private set; }

        public TChromosome[] SampleSet { get; private set; }

        public TChromosome[] ResultSet { get; private set; }



        public Engine( int populationSize, TInputType input, FnDeviationCalculator deviationFunction, FnEvolver evolver, FnChromosomeCreator chromosomeCreator )
        {
            PopulationSize = populationSize;
            InputConfig = input;
            DeviationFunction = deviationFunction;
            Evolver = evolver;
            ChromosomeCreator = chromosomeCreator;

            SampleSet = new TChromosome[PopulationSize];
            ResultSet = new TChromosome[PopulationSize];
            _sampleSums = new double[PopulationSize];

            Initialize();
        }

        /// <summary>
        /// Resets the populations to initial values. Does not re-allocate any memory.
        /// </summary>
        public void Initialize()
        {
            for (int i = 0; i < PopulationSize; i++)
            {
                var chromo = ChromosomeCreator( InputConfig, true );
                DeviationFunction( InputConfig, chromo, default );
                SampleSet[i] = chromo;

                ResultSet[i] = ChromosomeCreator( InputConfig, false );
            }

            // Make sure new SampleSet is ready for a binary search
            ProcessSampleSet();
            IterationCount = 0;
        }


        /// <summary>
        /// Advance the evolution through one generation.
        /// 
        /// If using Elitism, the return value's <see cref="Chromosome.Deviation"/> value will
        /// be no larger than the previous value returned by this method.
        /// </summary>
        /// <returns>
        /// The <see cref="Chromosome"/> indicative of the best/lowest
        /// <see cref="Chromosome.Deviation"/> value.
        /// </returns>
        public TChromosome EvolveOneGeneration()
        {
            // Elitism
            if (UseElitism)
                Best.CopyTo( ResultSet[0] );

            // Generation
            for (int i = UseElitism ? 1 : 0; i < PopulationSize; i++)
            {
                // start can be parallel

                var output = ResultSet[i];
                Evolver( Sample, output );
                DeviationFunction( InputConfig, output, default );

                // end can be parallel
            }

            // swap lists
            var tmp = SampleSet;
            SampleSet = ResultSet;
            ResultSet = tmp;

            // Make sure new SampleSet is ready for a binary search
            ProcessSampleSet();
            IterationCount++;

            return Best;
        }


        /// <summary>
        /// Primary function is to create the _sampleSums array so that the
        /// <see cref="SampleSet"/> can be searched using a simple binary search.
        /// 
        /// Secondarily finds the <see cref="Best"/> <see cref="Chromosome"/> in the
        /// <see cref="SampleSet"/> .
        /// </summary>
        /// <remarks>
        /// TODO: Refactor to handle a <see cref="Chromosome.Deviation"/> ==0 situation, which
        /// would <see cref="DivideByZeroException"/> now.
        /// 
        /// Propose to immediately return Best = First chromosome where Deviation ==0, leaving
        /// SampleSet in an invalid state. This is tail-wagging-dog justified by stating that
        /// there could not possibly be a better chromosome, therefore the sample set's state is
        /// irrelevant.
        /// 
        /// This of course leaves the debugging of the invalid SampleSet more interesting...
        /// </remarks>
        private void ProcessSampleSet()
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


        /// <summary>
        /// Callback provided to the Evolver when it needs to sample the set for a chromosome.
        /// 
        /// Executes in O(ln) time.
        /// 
        /// This routine is re-entrant, but depends on a stable SampleSet. The application must
        /// coordinate access to the SampleSet, as this routine ASSUMES stability for the sake
        /// of performance.
        /// </summary>
        /// <returns>A chromosome selected (pseudo)randomly from the set</returns>
        public TChromosome Sample()
        {
            int low = 0;
            int mid = 0; // If all fails in the loop, just assume 0 even if array is empty
            int high = PopulationSize;

            var selection = _rng.NextDouble() * _sampleSetSumDeviations;

            while (low < high)
            {
                mid = low + (high - low) / 2;

                if (selection > _sampleSums[mid])
                    low = mid + 1;
                else
                    high = mid - 1;
            }
            if (low == high) mid = high;

            return SampleSet[mid];
        }

    }
}
